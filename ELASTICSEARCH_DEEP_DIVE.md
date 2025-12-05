# 📚 Elasticsearch - სიღრმისეული ტექნიკური გაიდი

## პრეზენტაციის მიზანი
ეს დოკუმენტი შეიცავს დეტალურ ახსნას Elasticsearch-ის ძირითადი კონცეფციების შესახებ, რომლებიც აუცილებელია პროექტის სრულად გასაგებად.

---

# 📑 სარჩევი

1. [Lucene Segments - რა არის და როგორ მუშაობს](#1-lucene-segments)
2. [Dynamic Mapping - JSON ანალიზი](#2-dynamic-mapping)
3. [Inverted Index - სწრაფი ძებნის საიდუმლო](#3-inverted-index)
4. [Scoring Algorithms - TF-IDF და BM25](#4-scoring-algorithms)
5. [Full-text vs Exact Match](#5-full-text-vs-exact-match)
6. [Aggregations - Size: 0](#6-aggregations)
7. [Single-Node vs Cluster](#7-single-node-vs-cluster)
8. [პრაქტიკული მაგალითები](#8-practical-examples)

---

# 1. Lucene Segments

## 1.1 რა არის Segment?

**Segment** არის Lucene-ის (Elasticsearch-ის ბაზა) ძირითადი შენახვის ერთეული - **Immutable (შეუცვლელი)** ფაილი დისკზე.

### სტრუქტურა:

```
┌─────────────────────────────────────┐
│         Elasticsearch Index         │
├─────────────────────────────────────┤
│           Shard (Partition)         │
├─────────────────────────────────────┤
│  Lucene Segment 1  (1000 docs)     │
│  Lucene Segment 2  (500 docs)      │
│  Lucene Segment 3  (200 docs)      │
│  Lucene Segment 4  (50 docs)       │
└─────────────────────────────────────┘
```

### Segment-ის შემადგენელი ფაილები:

```
segment_0/
│
├── _0.si      Segment Info (metadata)
├── _0.fdx     Field Index (სად არის ველები)
├── _0.fdt     Field Data (თავად data)
├── _0.tim     Terms Dictionary (ყველა unique term)
├── _0.tip     Terms Index (term-ების index)
├── _0.doc     Document IDs
├── _0.pos     Positions (სად არის term დოკუმენტში)
└── _0.pay     Payloads (დამატებითი info)
```

## 1.2 როგორ იქმნება Segment?

```
1. Document ინდექსდება
   ↓
2. Memory Buffer-ში გადადის (არ არის searchable)
   ┌─────────────────┐
   │  Memory Buffer  │  100-200 documents
   │  (In RAM)       │
   └─────────────────┘
   ↓
3. Refresh (default: every 1 second)
   ↓
4. Disk-ზე იწერება როგორც Segment
   ┌─────────────────┐
   │  New Segment    │  ახლა searchable!
   │  (On Disk)      │
   └─────────────────┘
```

**მაგალითი:**

```csharp
// თქვენ ინდექსავთ 100 product-ს:
for (int i = 0; i < 100; i++)
{
    await _client.IndexDocumentAsync(new Product { Name = $"Product {i}" });
}

// რა ხდება:
// 1. 100 document → Memory Buffer
// 2. 1 second later → Refresh
// 3. Segment_1 იქმნება (100 docs) → ახლა searchable!
```

## 1.3 რატომ არის Immutable?

Segment შექმნის შემდეგ **არ იცვლება!**

### Update-ის პროცესი:

```csharp
// თქვენ update-ს აკეთებთ:
product.Price = 999.99;
await _client.UpdateAsync<Product>(product.Id, u => u.Doc(product));

// რეალურად რა ხდება:
// ❌ Segment-ში არ ხდება in-place update
// ✅ რეალური პროცესი:

1. ძველი document მიიღება special flag: "DELETED"
   Segment 1: [Doc1, Doc2-DELETED, Doc3]
   
2. ახალი version ინდექსდება ახალ Segment-ში
   Segment 2: [Doc2-NEW]
   
3. Search დროს:
   • Doc2-DELETED გამოტოვდება
   • Doc2-NEW ჩაითვლება
   
4. Background Merge-ის დროს:
   • Doc2-DELETED physically იშლება
   • Segments უერთდება
```

## 1.4 Segment Merge

### როდის ხდება Merge?

**ავტომატური Triggers:**

```
1. ძალიან ბევრი Segment:
   [S1] [S2] [S3] ... [S50] ← ძალიან ბევრია!
   → Search ნელდება (უნდა შეამოწმოს 50 segment)
   → Merge!

2. Deleted Documents:
   Segment: [Doc1, Doc2-DEL, Doc3-DEL, Doc4, Doc5-DEL]
   Deleted: 60% ← ბევრი deleted doc
   → Merge და გასუფთავება!

3. Size Tiers:
   Small segments → Merge ხშირად
   Large segments → Merge იშვიათად
```

### Merge Process:

```
Before Merge:
┌──────┐ ┌──────┐ ┌──────┐
│ S1   │ │ S2   │ │ S3   │
│10KB  │ │15KB  │ │8KB   │
│Doc1  │ │Doc4  │ │Doc6  │
│Doc2  │ │Doc5  │ │Doc7  │
│Doc3  │ │      │ │      │
└──────┘ └──────┘ └──────┘
Some docs are DELETED

Merge Process:
1. წაიკითხავს ყველა segment-ს
2. წაშლის DELETED documents-ს
3. ქმნის ახალ დიდ segment-ს

After Merge:
┌──────────────┐
│ Merged S1-3  │
│    30KB      │
│ Doc1, Doc3   │
│ Doc4, Doc5   │
│ Doc6, Doc7   │
└──────────────┘
Old segments deleted!
```

### კონფიგურაცია:

```json
{
  "index": {
    "merge": {
      "policy": {
        "max_merged_segment": "5gb",
        "segments_per_tier": 10,
        "deletes_pct_allowed": 33
      }
    }
  }
}
```

**რეკომენდაცია:** 
- ✅ **Default settings კარგია** - Elasticsearch თავად ართმევს merge-ებს
- ✅ Manual Force Merge მხოლოდ read-only indices-ზე
- ❌ არ გააკეთო force merge active indices-ზე

## 1.5 Terms Dictionary და Terms Index

### Terms Dictionary (_0.tim)

**ყველა unique term-ის სია + metadata:**

```
┌────────────────────────────────────────────┐
│           Terms Dictionary                 │
├────────────────────────────────────────────┤
│ Term      │ Doc Freq │ Pointer to Postings│
├───────────┼──────────┼────────────────────┤
│ "apple"   │    450   │  → offset: 1024    │
│ "banana"  │    280   │  → offset: 2048    │
│ "macbook" │    450   │  → offset: 5120    │
│ "pro"     │    320   │  → offset: 7168    │
└───────────┴──────────┴────────────────────┘
```

**რას შეიცავს:**
- Term text
- Document Frequency (რამდენ doc-ში გვხვდება)
- Total Term Frequency (სულ რამდენჯერ გვხვდება)
- Pointer to Posting List

### Terms Index (_0.tip)

**Terms Dictionary-ის Index (in-memory):**

```
Problem: Terms Dictionary შეიძლება იყოს 100GB
Solution: შევქმნათ small in-memory index prefixes-ისთვის

Terms Index (RAM, 100MB):
┌─────────────────────────────────┐
│ Prefix │ Offset in .tim file    │
├────────┼────────────────────────┤
│ "a"    │ → 0                    │
│ "b"    │ → 10,240               │
│ "m"    │ → 102,400              │
│ "z"    │ → 512,000              │
└────────┴────────────────────────┘
```

### როგორ მუშაობს Lookup:

```
Query: "macbook"
      ↓
1. Terms Index (RAM): Find "m" prefix
   "m" → offset: 102,400
      ↓
2. Jump to offset 102,400 in .tim (disk)
      ↓
3. Read relevant section:
   [mac, macbook, machine, ...]
      ↓
4. Binary Search: Find "macbook"
   Found! → pointer: 50,000
      ↓
5. Read Posting List at offset 50,000
   [doc1, doc5, doc12, doc18, ...]
```

**Performance:**
- Terms Index (100MB) → RAM (instant lookup)
- Terms Dictionary (100GB) → Disk (read only needed part)
- Total time: ~1ms ✅

---

# 2. Dynamic Mapping

## 2.1 რა არის Dynamic Mapping?

Elasticsearch **ავტომატურად** ამოიცნობს JSON field types-ს.

### მაგალითი:

```csharp
// თქვენ ინდექსავთ:
var product = new {
    name = "MacBook Pro",              // String
    price = 1999.99,                   // Number
    inStock = true,                    // Boolean
    tags = new[] { "laptop", "apple" },// Array
    createdAt = "2025-01-15T10:30:00" // Date string
};

await _client.IndexDocumentAsync(product);
```

**Elasticsearch ქმნის mapping-ს:**

```json
{
  "mappings": {
    "properties": {
      "name": {
        "type": "text",
        "fields": {
          "keyword": { "type": "keyword" }
        }
      },
      "price": { "type": "float" },
      "inStock": { "type": "boolean" },
      "tags": {
        "type": "text",
        "fields": {
          "keyword": { "type": "keyword" }
        }
      },
      "createdAt": { "type": "date" }
    }
  }
}
```

## 2.2 Detection Rules:

| JSON Value | Detected Type |
|-----------|--------------|
| `"text"` | `text` + `keyword` sub-field |
| `123` | `long` |
| `12.34` | `float` |
| `true`/`false` | `boolean` |
| `"2025-01-15"` | `date` (ISO format) |
| `["a", "b"]` | array of detected type |
| `{ "nested": {} }` | `object` |

## 2.3 რას აანალიზებს?

### Text Analysis Process:

```
Input: "MacBook Pro M3"

1. Tokenization:
   "MacBook Pro M3" → ["MacBook", "Pro", "M3"]

2. Lowercase Filter:
   ["MacBook", "Pro", "M3"] → ["macbook", "pro", "m3"]

3. Remove Punctuation:
   ["macbook", "pro", "m3"] → ["macbook", "pro", "m3"]

4. Store in Inverted Index:
   macbook → [doc1]
   pro     → [doc1]
   m3      → [doc1]
```

### პრობლემა Dynamic Mapping-თან:

```csharp
// ❌ Wrong type detection:
var product = new {
    price = "1999.99"  // String, not number!
};

// Elasticsearch creates:
// "price": { "type": "text" } ← Wrong!

// ✅ Better: Explicit mapping
await _client.Indices.CreateAsync("products", c => c
    .Map<Product>(m => m
        .Properties(p => p
            .Number(n => n.Name(x => x.Price).Type(NumberType.Float))
        )
    )
);
```

---

# 3. Inverted Index

## 3.1 რა არის Inverted Index?

**Traditional Index:**
```
Page 1 → "Elasticsearch basics"
Page 2 → "Indexing documents"
Page 3 → "Lucene segments"
```

**Inverted Index:**
```
elasticsearch → Page 1
indexing     → Page 2
lucene       → Page 3
segments     → Page 3
```

## 3.2 Elasticsearch-ში:

```
Documents:
Doc 1: "MacBook Pro M3 is fast"
Doc 2: "MacBook Air is lightweight"
Doc 3: "Pro Display XDR"

Inverted Index:
┌───────────┬──────────────┬───────────┬──────────┐
│   Term    │ Document IDs │ Frequency │ Position │
├───────────┼──────────────┼───────────┼──────────┤
│ macbook   │ [1, 2]       │     2     │ [0, 0]   │
│ pro       │ [1, 3]       │     2     │ [1, 0]   │
│ m3        │ [1]          │     1     │ [2]      │
│ fast      │ [1]          │     1     │ [4]      │
│ air       │ [2]          │     1     │ [1]      │
│ display   │ [3]          │     1     │ [1]      │
│ xdr       │ [3]          │     1     │ [2]      │
└───────────┴──────────────┴───────────┴──────────┘
```

## 3.3 Lookup Process:

```
Query: "MacBook Pro"
      ↓
1. Analysis: "MacBook Pro" → ["macbook", "pro"]
      ↓
2. Inverted Index Lookup (Hash Table):
   
   "macbook" → [doc1, doc2]  ← O(1) lookup!
   "pro"     → [doc1, doc3]  ← O(1) lookup!
      ↓
3. Combine (Boolean OR):
   Union: [1, 2, 3]
      ↓
4. Score Each:
   Doc1: both terms → High (15.9)
   Doc2: only macbook → Medium (4.6)
   Doc3: only pro → Medium (3.2)
      ↓
5. Sort: [Doc1, Doc2, Doc3]
```

## 3.4 Performance სარგებელი:

```
Without Inverted Index (like SQL LIKE):
SELECT * FROM products WHERE name LIKE '%MacBook%';
→ Scan all 1,000,000 rows → O(n)
→ Time: ~5 seconds

With Inverted Index:
"macbook" → [doc1, doc5, doc12] ← Hash lookup O(1)
→ Time: ~1 millisecond ✅

Speed improvement: 5000x faster!
```

## 3.5 Multiple Segments:

```
Query: "macbook"
      ↓
Parallel Lookup in each segment:

┌──────────────────┐
│   Segment 1      │ → "macbook" → [1, 5]
├──────────────────┤
│   Segment 2      │ → "macbook" → [12, 18]
├──────────────────┤
│   Segment 3      │ → "macbook" → [25]
├──────────────────┤
│   Segment 4      │ → "macbook" → []
└──────────────────┘
      ↓
Merge: [1, 5, 12, 18, 25]
      ↓
Score & Sort
```

---

# 4. Scoring Algorithms

## 4.1 რატომ არის საჭირო Scoring?

```
Query: "MacBook Pro"

Results:
Doc1: "MacBook Pro M3 16GB" ← Best match!
Doc2: "MacBook Air"          ← Good match
Doc3: "Pro camera"           ← Weak match
Doc4: "Microsoft Surface Pro"← False positive

როგორ დავალაგოთ relevance-ის მიხედვით?
→ Scoring Algorithms!
```

## 4.2 TF-IDF (Term Frequency - Inverse Document Frequency)

### TF (Term Frequency):

**რამდენჯერ გვხვდება term document-ში**

```
Formula: TF = (term count in doc) / (total terms in doc)

Example:
Doc: "MacBook Pro M3 MacBook is great" (6 words)
     "macbook" appears 2 times

TF("macbook") = 2 / 6 = 0.33
```

### IDF (Inverse Document Frequency):

**რამდენად იშვიათია term**

```
Formula: IDF = log(total docs / docs containing term)

Example: 10,000 total documents
• "macbook": in 100 documents
• "the": in 9,500 documents

IDF("macbook") = log(10000/100) = log(100) = 4.6  ← High!
IDF("the") = log(10000/9500) = log(1.05) = 0.05   ← Low!

Idea: იშვიათი terms უფრო მნიშვნელოვანია!
```

### TF-IDF = TF × IDF:

```
Doc: "MacBook is great"

Term "macbook":
TF = 1/3 = 0.33
IDF = 4.6
Score = 0.33 × 4.6 = 1.52 ← High!

Term "the" (if present):
TF = 1/3 = 0.33
IDF = 0.05
Score = 0.33 × 0.05 = 0.017 ← Very low!

Conclusion: Rare terms matter more!
```

## 4.3 BM25 (Best Matching 25)

**Elasticsearch-ის default scorer (TF-IDF-ის გაუმჯობესებული ვერსია)**

```
Formula:
BM25 = IDF(term) × 
       (TF × (k1 + 1)) /
       (TF + k1 × (1 - b + b × (docLen / avgDocLen)))

Parameters:
k1 = 1.2  (TF saturation)
b = 0.75  (length normalization)
```

### BM25 vs TF-IDF:

**1. Diminishing Returns (TF Saturation):**

```
TF-IDF:
Term appears 1 time  → +1.0
Term appears 2 times → +2.0
Term appears 10 times→ +10.0 ← Unrealistic!

BM25:
Term appears 1 time  → +1.0
Term appears 2 times → +1.6
Term appears 5 times → +1.8
Term appears 10 times→ +1.9  ← Saturates!

After ~5 occurrences, score doesn't grow much
→ Prevents keyword stuffing!
```

**2. Document Length Normalization:**

```
Doc1: "MacBook" (1 word, short)
Doc2: "MacBook is great laptop... (100 words, long)

TF-IDF:
Doc1: TF = 1/1 = 1.0   → High
Doc2: TF = 1/100 = 0.01 → Very low
Problem: Long docs penalized too much!

BM25:
• Considers average document length
• Less penalty for long documents
• Fairer scoring ✅
```

## 4.4 TF vs IDF - რომელი უფრო მნიშვნელოვანია?

**პასუხი: IDF ბევრად უფრო მნიშვნელოვანია!**

### მაგალითი:

```csharp
// Query: "MacBook Pro"

// Term: "macbook" (moderate rarity)
IDF = 4.6
TF = 1
Score contribution = 4.6 × 1.0 = 4.6

// Term: "the" (very common, if present)
IDF = 0.05
TF = 5
Score contribution = 0.05 × 1.5 = 0.075

// Conclusion:
// "macbook" (1 occurrence) > "the" (5 occurrences)
// Because IDF weight is dominant!
```

### რატომ IDF უპირატესი:

```
Common words ("the", "is", "a"):
• გვხვდება ყველა document-ში
• არ გვეუბნება არაფერს content-ის შესახებ
• Low information value
→ Low IDF → Low score ✅

Rare words ("quantum", "elasticsearch"):
• გვხვდება specific documents-ში
• High information value
• Distinguishes documents
→ High IDF → High score ✅

User Intent:
Query: "MacBook Pro M3"
• "M3" - rare, very specific → IDF: 8.1 ← Most important!
• "MacBook" - moderate → IDF: 4.6
• "Pro" - moderate → IDF: 3.2
```

### Practical Example:

```
Query: "MacBook Pro M3"

Doc1: "MacBook Pro M3" (all 3 terms)
• macbook: 4.6
• pro: 3.2
• m3: 8.1
Total: 15.9 ← Best!

Doc2: "MacBook Pro laptop"
• macbook: 4.6
• pro: 3.2
• m3: 0 (missing!)
Total: 7.8

Doc3: "MacBook Air"
• macbook: 4.6
• pro: 0
• m3: 0
Total: 4.6

Ranking: Doc1 > Doc2 > Doc3 ✅
```

---

# 5. Full-text vs Exact Match

## 5.1 Text Field (Full-text Search)

```csharp
// Mapping:
.Text(t => t.Name(n => n.Name))

// Document:
{ "name": "MacBook Pro M3" }

// Analysis process:
"MacBook Pro M3"
→ Tokenize: ["MacBook", "Pro", "M3"]
→ Lowercase: ["macbook", "pro", "m3"]
→ Store in Inverted Index
```

### რა queries იპოვის:

```csharp
✅ "MacBook"          → Match
✅ "macbook"          → Match (case insensitive)
✅ "MACBOOK"          → Match
✅ "MacBook Pro"      → Match
✅ "Pro M3"           → Match
✅ "macbook pro m3"   → Match
✅ "Mac"              → Partial match (prefix)
```

## 5.2 Keyword Field (Exact Match)

```csharp
// Mapping:
.Keyword(k => k.Name(n => n.Category))

// Document:
{ "category": "Electronics" }

// NO analysis! Stored as-is
"Electronics" → stored exactly
```

### რა queries იპოვის:

```csharp
✅ "Electronics"      → Match (exact)

❌ "electronics"      → No match (different case!)
❌ "Elec"            → No match (incomplete)
❌ "ELECTRONICS"     → No match
```

## 5.3 Comparison:

```csharp
var product = new Product
{
    Name = "MacBook Pro M3",        // Text field
    Category = "Electronics",       // Keyword field
    Brand = "Apple"                 // Keyword field
};

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Text Search:
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
await _client.SearchAsync<Product>(s => s
    .Query(q => q
        .Match(m => m.Field(f => f.Name).Query("macbook"))
    )
);
// ✅ იპოვის! (case insensitive, analyzed)

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// Exact Match:
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━
await _client.SearchAsync<Product>(s => s
    .Query(q => q
        .Term(t => t.Field(f => f.Category).Value("Electronics"))
    )
);
// ✅ იპოვის! (exact match)

await _client.SearchAsync<Product>(s => s
    .Query(q => q
        .Term(t => t.Field(f => f.Category).Value("electronics"))
    )
);
// ❌ არ იპოვის! (case sensitive)
```

## 5.4 Hybrid Approach (რეკომენდებული):

```csharp
// Name field - BOTH text და keyword:
.Text(t => t
    .Name(n => n.Name)
    .Fields(f => f
        .Keyword(k => k.Name("keyword"))
    )
)

// ახლა გაქვს:
// product.name → text (for search)
// product.name.keyword → keyword (for sort/filter)

// Search:
.Match(m => m.Field(f => f.Name).Query("macbook"))

// Sort:
.Sort(s => s.Ascending(f => f.Name.Suffix("keyword")))

// Filter exact:
.Term(t => t.Field(f => f.Name.Suffix("keyword")).Value("MacBook Pro"))
```

## 5.5 Use Cases:

| Field Type | Use Case | Example |
|-----------|----------|---------|
| **text** | Full-text search | Product names, descriptions |
| **keyword** | Exact filtering | Categories, status, tags |
| **keyword** | Sorting | Sort by brand |
| **keyword** | Aggregations | Count by category |
| **text** | Autocomplete | Search as you type |
| **keyword** | IDs | SKU, user ID |

---

# 6. Aggregations

## 6.1 Size: 0 - რას ნიშნავს?

### Normal Search:

```csharp
// Default (size: 10):
var response = await _client.SearchAsync<Product>(s => s
    .Size(10)
    .Query(q => q.MatchAll())
);

// Response:
{
  "hits": {
    "total": { "value": 1000 },
    "hits": [
      { "_source": { "name": "Product 1" } },
      { "_source": { "name": "Product 2" } },
      ...
      { "_source": { "name": "Product 10" } }
    ]
  }
}
```

### Aggregation Only (size: 0):

```csharp
var response = await _client.SearchAsync<Product>(s => s
    .Size(0)  // ← არ დააბრუნო documents!
    .Aggregations(a => a
        .Terms("categories", t => t.Field(f => f.Category))
    )
);

// Response:
{
  "hits": {
    "total": { "value": 1000 },
    "hits": []  // ← ცარიელი!
  },
  "aggregations": {
    "categories": {
      "buckets": [
        { "key": "Electronics", "doc_count": 450 },
        { "key": "Clothing", "doc_count": 320 }
      ]
    }
  }
}
```

## 6.2 რატომ Size: 0?

### Performance:

```
Normal (size: 10):
1. Execute query
2. Fetch 10 documents from disk ← Slow
3. Serialize to JSON ← CPU intensive
4. Send over network ← 50KB response
Total: ~200ms

Aggregation (size: 0):
1. Execute query
2. Calculate aggregations (in-memory) ← Fast
3. Return only stats ← 2KB response
Total: ~50ms ✅

Speed: 4x faster!
Size: 25x smaller!
```

## 6.3 Use Case:

```csharp
// Dashboard Statistics:
var response = await _client.SearchAsync<Product>(s => s
    .Size(0)  // გვინდა მხოლოდ statistics
    .Aggregations(a => a
        .Average("avg_price", avg => avg.Field(f => f.Price))
        .Sum("total_revenue", sum => sum.Field(f => f.Price))
        .Terms("top_categories", t => t
            .Field(f => f.Category)
            .Size(5)
        )
    )
);

// გვინდა:
// • Average price: 1299.99
// • Total revenue: 1,500,000
// • Top 5 categories
//
// არ გვინდა 10,000 product-ის სია!
```

## 6.4 როდის რა:

| Scenario | Size | Explanation |
|----------|------|-------------|
| Show products | `10` | Need actual documents |
| Dashboard stats | `0` | Only aggregations |
| Count by category | `0` | Only buckets |
| Products + stats | `10` | Both documents and aggs |
| Export data | `1000` | Many documents |

---

# 7. Single-Node vs Cluster

## 7.1 discovery.type=single-node

```yaml
# docker-compose.yml
services:
  elasticsearch:
    environment:
      - discovery.type=single-node  # ← Single node mode
```

### რას ნიშნავს:

```
┌─────────────────────────────────┐
│   Single Elasticsearch Node     │
│                                 │
│  • Port: 9200                   │
│  • No clustering                │
│  • No node discovery            │
│  • Development/Testing          │
│  • Starts in 10 seconds ✅      │
└─────────────────────────────────┘
```

## 7.2 Without single-node (Cluster mode):

```yaml
# Production (3 nodes):
services:
  es-node-1:
    environment:
      - cluster.name=prod-cluster
      - node.name=node-1
      - discovery.seed_hosts=node-2,node-3
      - cluster.initial_master_nodes=node-1,node-2,node-3
  
  es-node-2:
    # ... same config ...
  
  es-node-3:
    # ... same config ...
```

### Cluster Structure:

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│  Node 1  │────▶│  Node 2  │────▶│  Node 3  │
│  Master  │     │  Data    │     │  Data    │
└──────────┘     └──────────┘     └──────────┘
     ▲                                   │
     └───────────────────────────────────┘
         Discovery & Health Check
```

## 7.3 პრობლემა single-node-ის გარეშე:

```bash
# Without single-node:
$ docker-compose up

Logs:
[ERROR] master not discovered yet
[ERROR] not enough master-eligible nodes
[WARN] waiting for other nodes...
# არასოდეს გამწვანდება! ❌

# With single-node:
$ docker-compose up

Logs:
[INFO] single-node mode detected
[INFO] skipping cluster discovery
[INFO] cluster health: GREEN
# მაშინვე მუშაობს! ✅
```

## 7.4 როდის რა გამოიყენო:

| Environment | Mode | Why |
|------------|------|-----|
| **Local Dev** | Single Node | Simple, fast startup |
| **Testing** | Single Node | No complexity |
| **Small Prod** | Single Node | Cost-effective |
| **Large Prod** | Cluster (3+) | High availability |
| **Mission Critical** | Cluster (5+) | Data redundancy |

## 7.5 Single-Node Limitations:

```
❌ No High Availability
   Node crashes → service down

❌ No Horizontal Scaling
   Can't add more nodes easily

❌ No Data Replication
   No redundancy

✅ Perfect for Development!
✅ Simple setup
✅ Fast startup
```

---

# 8. პრაქტიკული მაგალითები

## 8.1 Complete Search Example

```csharp
// Query: "MacBook Pro M3"

// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
// რა ხდება სტეპ-ბაი-სტეპ:
// ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

// 1. Query Analysis:
"MacBook Pro M3"
→ Tokenize: ["MacBook", "Pro", "M3"]
→ Lowercase: ["macbook", "pro", "m3"]

// 2. Inverted Index Lookup (parallel per segment):
Segment 1:
  "macbook" → [doc1, doc5]
  "pro"     → [doc1, doc3]
  "m3"      → [doc1]

Segment 2:
  "macbook" → [doc12, doc18]
  "pro"     → [doc12]
  "m3"      → [doc18]

// 3. Merge results:
Documents: [1, 3, 5, 12, 18]

// 4. Scoring (BM25):
Doc1:  macbook(4.6) + pro(3.2) + m3(8.1) = 15.9 ← Best!
Doc5:  macbook(4.6) = 4.6
Doc3:  pro(3.2) = 3.2
Doc12: macbook(4.6) + pro(3.2) = 7.8
Doc18: macbook(4.6) + m3(8.1) = 12.7

// 5. Sort by score:
[Doc1(15.9), Doc18(12.7), Doc12(7.8), Doc5(4.6), Doc3(3.2)]

// 6. Return top 10 (or size parameter)
```

## 8.2 Bulk Indexing Performance

```csharp
// ❌ Bad: Individual requests
for (int i = 0; i < 1000; i++)
{
    await _client.IndexDocumentAsync(products[i]);
}
// 1000 HTTP requests
// Time: ~30 seconds

// ✅ Good: Bulk request
var bulkDescriptor = new BulkDescriptor();
foreach (var product in products)
{
    bulkDescriptor.Index<Product>(i => i.Document(product));
}
await _client.BulkAsync(bulkDescriptor);
// 1 HTTP request
// Time: ~1 second ✅
// 30x faster!
```

## 8.3 Search with Aggregations

```csharp
var response = await _client.SearchAsync<Product>(s => s
    .Query(q => q
        .Match(m => m.Field(f => f.Name).Query("laptop"))
    )
    .Aggregations(a => a
        .Terms("brands", t => t
            .Field(f => f.Brand.Suffix("keyword"))
            .Size(10)
        )
        .Range("price_ranges", r => r
            .Field(f => f.Price)
            .Ranges(
                rr => rr.To(500),
                rr => rr.From(500).To(1000),
                rr => rr.From(1000)
            )
        )
    )
);

// Results:
// • 450 laptops found
// • Top brands: Apple (120), Dell (80), HP (60)
// • Price ranges: 
//   - <$500: 150 products
//   - $500-$1000: 200 products
//   - >$1000: 100 products
```

## 8.4 Update Process

```csharp
// Update product:
product.Price = 1499.99;
await _client.UpdateAsync<Product>(product.Id, u => u.Doc(product));

// რა ხდება Segment-ში:

Before:
Segment 1: [Product1{price:1999}, Product2, Product3]

After Update:
Segment 1: [Product1-DELETED{price:1999}, Product2, Product3]
Segment 2: [Product1-NEW{price:1499}]

Search result: Product1-NEW (Product1-DELETED ignored)

After Merge:
Merged Segment: [Product1-NEW{price:1499}, Product2, Product3]
```

## 8.5 Force Merge Example

```csharp
// Scenario: Old logs that won't change

// Before merge:
// Segment 1: 1000 docs, 200 deleted (20%)
// Segment 2: 800 docs, 150 deleted (19%)
// Segment 3: 500 docs, 100 deleted (20%)
// Total: 5 segments, 1.5GB

await _client.Indices.ForceMergeAsync("logs-2024-12", f => f
    .MaxNumSegments(1)
);

// After merge:
// Segment 1: 1850 docs (no deleted)
// Total: 1 segment, 900MB ✅
// Space saved: 600MB
// Search speed: 2x faster
```

---

# 📊 რეზიუმე: ძირითადი კონცეფციები

## Core Concepts:

| კონცეფცია | რას აკეთებს | რატომ მნიშვნელოვანი |
|-----------|-------------|-------------------|
| **Lucene Segment** | Immutable file on disk | ბაზის შენახვა და merge |
| **Terms Dictionary** | ყველა term + metadata | Term lookup საფუძველი |
| **Terms Index** | In-memory prefixes | სწრაფი disk jump |
| **Inverted Index** | Term → Doc IDs | O(1) search speed |
| **Dynamic Mapping** | Auto-detect types | სწრაფი დაწყება |
| **TF-IDF** | Term frequency × Rarity | Relevance scoring |
| **BM25** | Improved TF-IDF | Default scorer |
| **IDF** | Rare terms weight | უფრო მნიშვნელოვანია TF-ზე |
| **Text Field** | Analyzed search | Full-text search |
| **Keyword Field** | Exact match | Filters, aggregations |
| **Size: 0** | Skip documents | Aggregations only |
| **Single-Node** | No clustering | Dev/Test setup |

---

# 🎯 Best Practices

## 1. Mapping:

```csharp
// ✅ Good: Explicit mapping
await _client.Indices.CreateAsync("products", c => c
    .Map<Product>(m => m.Properties(p => p
        .Text(t => t.Name(n => n.Name)
            .Fields(f => f.Keyword(k => k.Name("keyword"))))
        .Keyword(k => k.Name(n => n.Category))
    ))
);

// ❌ Bad: Rely on dynamic mapping for production
```

## 2. Search:

```csharp
// ✅ Good: Match query for text fields
.Match(m => m.Field(f => f.Name).Query("macbook"))

// ✅ Good: Term query for keyword fields
.Term(t => t.Field(f => f.Category).Value("Electronics"))

// ❌ Bad: Term query on text field
.Term(t => t.Field(f => f.Name).Value("MacBook Pro"))
```

## 3. Bulk Operations:

```csharp
// ✅ Good: Batch size 1000-5000
var bulkDescriptor = new BulkDescriptor();
foreach (var item in items.Take(1000))
{
    bulkDescriptor.Index<T>(i => i.Document(item));
}

// ❌ Bad: Too large batches (>10000)
```

## 4. Segment Management:

```csharp
// ✅ Good: Let Elasticsearch auto-merge
// ✅ Good: Force merge read-only indices
await _client.Indices.ForceMergeAsync("logs-2024");

// ❌ Bad: Force merge active indices
// ❌ Bad: Too frequent force merge
```

## 5. Aggregations:

```csharp
// ✅ Good: size: 0 for stats only
.Size(0).Aggregations(a => a.Average(...))

// ✅ Good: Keyword fields for aggregations
.Terms("categories", t => t.Field(f => f.Category))

// ❌ Bad: Text fields for aggregations
.Terms("names", t => t.Field(f => f.Name)) // Wrong!
```

---

# 📚 დამატებითი რესურსები

## პროექტში:

1. `MappingService.cs` - Mapping examples
2. `SearchService.cs` - Search queries
3. `DocumentIndexingService.cs` - Bulk operations
4. `TextAnalysisService.cs` - Analysis examples

## დოკუმენტაცია:

- Elasticsearch Official Docs: https://www.elastic.co/guide
- Lucene Core: https://lucene.apache.org
- BM25 Algorithm: https://en.wikipedia.org/wiki/Okapi_BM25

---

# ❓ კითხვები პრეზენტაციისთვის

## შესაძლო კითხვები:

1. **რა არის Lucene Segment და რატომ არის Immutable?**
   - პასუხი: ფაილი დისკზე, immutable performance-ისთვის

2. **რა განსხვავებაა Text და Keyword field-ებს შორის?**
   - პასუხი: Text analyzed, Keyword exact match

3. **რატომ არის Inverted Index სწრაფი?**
   - პასუხი: Hash table O(1) lookup

4. **რა არის BM25 და რატომ უკეთესია TF-IDF-ზე?**
   - პასუხი: TF saturation და length normalization

5. **როდის გამოიყენებთ size: 0?**
   - პასუხი: Aggregations only, performance

6. **რა არის segment merge?**
   - პასუხი: combines segments, removes deleted docs

---

**წარმატებები პრეზენტაციაში! 🎉**

