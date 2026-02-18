# 🚀 Elasticsearch Project Presentation: Fundamental Guide

Practical guide: How to transfer SQL data to Elasticsearch, how the system works "under the hood," and how to build a high-performance search system with .NET.

---

## 🏗️ 1. Elasticsearch Philosophy and Fundamentals

Elasticsearch is not a conventional database. Its main strength stands on three pillars:

### 🔍 a) Inverted Index
This is the "heart" of Elasticsearch. Imagine the back pages of a book where words are arranged alphabetically with page numbers indicated.
*   **SQL:** Searching is like reading the entire book from start to finish (slow).
*   **Elastic:** The system pre-creates a list: `laptop -> [Document 1, 29, 105]`.
*   Searching happens directly in this list, providing instant results across millions of records.

### 🧩 b) Distributed Architecture (Shards & Replicas)
*   **Shard:** An index is divided into independent "bricks." When you search, all shards work **in parallel**.
*   **Replica:** A copy of a shard. If one server fails, the system retrieves data from the copy.
*   **Distribution:** The `hash(_id) % shards_count` formula determines which shard a document will land in.

### ⚡ c) Memory Management (RAM & OS Cache)
*   **In-Memory Buffer:** New data first lands in RAM.
*   **Filesystem Cache:** `Refresh` (e.g., once every 5 seconds) moves data from RAM into **segments**.
*   **Segments:** Small files of the inverted index. The more RAM available, the more segments are stored in the OS cache, and searching avoids the disk entirely.

---

## 🧪 2. Analysis Pipeline

Analysis is the process of "preparing" text before it is written into the inverted index.

### ⚙️ How does an Analyzer work?
1.  **Tokenizer:** Cutting text (e.g.: "MacBook Pro" -> `macbook`, `pro`).
2.  **Filters:**
    *   `Lowercase`: Converting letters to lowercase.
    *   `Stop Words`: Removing unnecessary words (a, the, and).
    *   `Snowball (Stemming)`: Reducing a word to its root (running -> run).
    *   `Synonyms`: Replacing synonyms (notebook = laptop).

### 💡 Important Rule:
Analysis happens on **both sides**:
*   **Indexing Time:** Optimized tokens are created.
*   **Search Time:** The user's search term is processed similarly so the "query" (key) fits the "index" (lock).

---

## 🛠️ 3. Index Mapping and Settings (Practice)

In our project, we use the `products-v2` index with improved synonyms and pagination.

### 📐 Mapping Example (NEST / JSON)
```json
PUT /products-v2
{
  "settings": {
    "number_of_shards": 3,
    "refresh_interval": "5s",
    "analysis": {
      "analyzer": {
        "product_name_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "stop", "snowball", "product_synonyms"]
        }
      },
      "filter": {
        "product_synonyms": {
          "type": "synonym",
          "synonyms": ["laptop, notebook, computer", "phone, smartphone"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "name": { 
        "type": "text", 
        "analyzer": "product_name_analyzer",
        "search_analyzer": "product_name_analyzer",
        "fields": { "keyword": { "type": "keyword" } }
      },
      "price": { "type": "float" },
      "category": { "type": "keyword" }
    }
  }
}
```

---

## 📊 4. Ranking and Relevance (_score)

Elasticsearch doesn't just tell you "if it found it or not," it tells you "how well it matches."

### ⚖️ How is the Score calculated?
*   **TF (Term Frequency):** The more often a word appears in a document, the higher the score.
*   **IDF (Inverse Document Frequency):** The rarer a word is in the entire database, the higher its "weight."
*   **Boost:** Artificial priority. e.g.: `.Field(p => p.Name, boost: 2.0)` means a match in the name is twice as important.

---

## 🩺 5. Debugging and Monitoring (Kibana Tools)

When searching doesn't work as you'd like, use these tools:

### 🔎 a) Search Profiler (`"profile": true`)
This is the "X-ray" of Elasticsearch. It shows the time for each condition:
*   `build_scorer`: How long the algorithm took to prepare.
*   `next_doc`: Time to find the next document.
*   You can see "Bottlenecks" (e.g., Wildcard search, which slows down the process).

### 📐 b) Explain API (`_explain`)
Answers the question: "Why did this document end up in first place?".
```json
GET /products-v2/_explain/29 { "query": { "match": { "name": "laptop" } } }
```
In the response, you will see the exact mathematical formula (Boost * IDF * TF).

### 🧪 c) Painless Lab
Painless is Elasticsearch's scripting language (similar to Java).
*   Used for dynamic calculations (e.g., calculating a discount during search).
*   In the Lab, you test safely before inserting it into a real Query.

---

## 🔄 6. Synchronization Flow (SQL → Elastic)

1.  **Extract:** Retrieving data from SQL Server (Hangfire Job).
2.  **Transform:** Transforming DTOs into a `Product` model (tokenization happens in Elastic).
3.  **Load:** Batch loading data using the `Bulk API` (e.g., 1000 records in one request).
4.  **Refresh:** After 5 seconds, the data becomes searchable.

---

## 💡 7. Tips and Best Practices
*   **Index Templates:** Use templates to automate the setup for new indices matching a pattern.
*   **Normalizers:** Use for `keyword` fields to enable case-insensitive searching/sorting without tokenization.
*   **Keyword vs Text:** Use `keyword` for filters/aggregations, `text` for searching.
*   **Zero-Downtime (Aliases):** Always use an alias (e.g., `products_alias`) in your application code. This allows you to reindex data into a new index (e.g., `products_v2`) and switch the alias instantly without any downtime.
*   **Performance:** Leave half of the RAM to the operating system for the Filesystem Cache.
*   **Painless:** Use only when you cannot pre-calculate data (e.g., distance calculation).

---

## 🧱 8. Advanced Index Management (Split, Shrink, Clone, Force Merge)

These operations help you change shard counts and optimize segments without fully reindexing.

### 🔹 Shrink (reduce shards)
- Use when an index is small and too many primary shards waste resources.
- Steps: set read-only → shrink → optionally unset read-only.
- API (our project):
  - POST /api/indexmanagement/shrink?sourceIndex=products-v2&targetIndex=products-v2-shrunk&targetShards=1
- Raw request (Elasticsearch):
```json
PUT /products-v2/_settings { "settings": { "index.blocks.write": true } }
POST /products-v2/_shrink/products-v2-shrunk { "settings": { "index.number_of_shards": 1, "index.number_of_replicas": 1 } }
```

### 🔹 Split (increase shards)
- Use when an index grew larger than planned and needs more primary shards.
- API (our project):
  - POST /api/indexmanagement/split?sourceIndex=products-v2&targetIndex=products-v2-split&targetShards=6
- Raw request:
```json
PUT /products-v2/_settings { "settings": { "index.blocks.write": true } }
POST /products-v2/_split/products-v2-split { "settings": { "index.number_of_shards": 6 } }
```

### 🔹 Clone (make an exact copy)
- For safe experiments or creating a frozen copy.
- API (our project):
  - POST /api/indexmanagement/clone?sourceIndex=products-v2&targetIndex=products-v2-clone
- Raw request:
```json
PUT /products-v2/_settings { "settings": { "index.blocks.write": true } }
POST /products-v2/_clone/products-v2-clone
```

### 🔹 Force Merge (segment optimization)
- Merges many small segments into fewer larger ones; use on static indices.
- API (our project):
  - POST /api/indexmanagement/forcemerge?indexName=products-v2&maxSegments=1
- Raw request:
```json
POST /products-v2/_forcemerge?max_num_segments=1
```

Notes:
- Split target shard count must be a multiple of the source primary shard count.
- Shrink target shards must divide the source primary shard count.
- Ensure the source index is read-only during split/shrink/clone.

---

## 📜 9. Elasticsearch Scripting (Painless)

Elasticsearch Scripting allows you to use custom logic for updates, searches, and data transformation.

### 🔹 Update By Query (Scripted Update)
- Bulk update documents matching a query using a script.
- API (our project):
  - POST /api/indexmanagement/bulk-update-script?category=laptops&discount=0.1
- Raw request:
```json
POST /products/_update_by_query
{
  "script": {
    "source": "ctx._source.price = ctx._source.price * (1.0 - params.discount)",
    "params": { "discount": 0.1 }
  },
  "query": { "term": { "category.keyword": "laptops" } }
}
```

### 🔹 Script Score (Dynamic Ranking)
- Adjust the relevance score based on dynamic factors (e.g., boosting products in stock).
- API (our project):
  - GET /api/advancedsearch/script-score?query=laptop
- Raw request:
```json
POST /products/_search
{
  "query": {
    "script_score": {
      "query": { "match": { "name": "laptop" } },
      "script": {
        "source": "double boost = doc['stock'].size() != 0 && doc['stock'].value > 0 ? 1.2 : 1.0; return _score * boost;"
      }
    }
  }
}
```

### 🔹 Script Fields (Calculated Values)
- Return values that are not stored in the index but are calculated on the fly.
- API (our project):
  - GET /api/advancedsearch/script-fields?query=laptop&vatRate=1.18
- Raw request:
```json
POST /products/_search
{
  "query": { "match": { "name": "laptop" } },
  "script_fields": {
    "price_with_vat": {
      "script": {
        "source": "doc['price'].value * params.vat",
        "params": { "vat": 1.18 }
      }
    }
  }
}
```

### 🔹 Runtime Fields (Search-time Mapping)
- Define and use fields that are calculated during the search without changing the index mapping.
- API (our project):
  - GET /api/advancedsearch/runtime-fields?priceThreshold=2000
- Raw request:
```json
POST /products/_search
{
  "runtime_mappings": {
    "is_expensive": {
      "type": "boolean",
      "script": {
        "source": "emit(doc['price'].value > params.threshold)",
        "params": { "threshold": 2000 }
      }
    }
  },
  "query": {
    "term": { "is_expensive": true }
  }
}
```

---

## ❓ Q&A
Elasticsearch is a tool that "works harder" during writing to be as lightweight and fast as possible during searching.
