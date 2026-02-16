# Elasticsearch პროექტის პრეზენტაცია

პრაქტიკული გზამკვლევი: როგორ გადავიტანოთ SQL მონაცემები Elasticsearch-ში, ავაწყოთ სწრაფი ძიება/აგრეგაციები და გავუშვათ ყველაფერი Docker-ით.

---

## შინაარსი
- პროექტის მიზანი და ღირებულება
- არქიტექტურა და მონაცემთა ნაკადი
- Elasticsearch vs SQL – როდის რა გამოვიყენოთ
- საბაზისო კონცეფციები (Index, Shard, Replica, Mapping, Analyzer)
- ინდექსის მეპინგი და ანალიზატორები (პრაქტიკული JSON)
- მონაცემების სინქრონიზაცია SQL→Elasticsearch (Bulk)
- API მაგალითები (Complex/Fuzzy/Autocomplete/Paginated)
- Kibana – სწრაფი ვალიდაცია და ვიზუალიზაცია
- აგრეგაციები – მაგალითები
- Docker Compose – გაშვება, მოცულობები, ქსელები
- Reindex – საუკეთესო პრაქტიკები
- Refresh Interval – ტიუნინგის რეკომენდაციები
- Keyword vs Text – სწორი არჩევანი
- Autocomplete – edge_ngram პრაქტიკაში
- Hangfire/Worker – საიმედო სინქრონიზაცია
- წარმადობა, სტაბილურობა, უსაფრთხოება
- Troubleshooting – ხშირად დასმული პრობლემები
- Go‑Live ჩეკლისტი
- ტერმინების ლექსიკონი
- Q&A

---

## პროექტის მიზანი და ღირებულება
- SQL Server-დან მონაცემების სინქრონიზაცია Elasticsearch-ში
- სწრაფი და მოქნილი ძიება (full‑text, ფაზური/ფაზა‑ფრაზა, ფაზიფიკაცია), ავტოშევსება, აგრეგაციები
- Docker‑ით მარტივი ადგილობრივი/სასერვერო გაშვება და ინფრასტრუქტურის მართვა
- მიზანი: მომხმარებელს მივაწოდოთ მაქსიმალურად სწრაფი ძიების გამოცდილება მინიმალური ინფრასტრუქტურული ხარჯით

---

## არქიტექტურა და მონაცემთა ნაკადი
- SQL Server – ძირითადი ტრანზაქციული საცავი
- Elasticsearch – ძიება და ანალიტიკა
- Hangfire Jobs/Worker – ფონური სინქრონიზაცია და ინდექსაცია (Bulk)
- API – ძიების endpoint‑ები მომხმარებლისთვის
- Docker Compose – სერვისების კომბინირებული გაშვება

მონაცემთა ნაკადი:
1) SQL → (SELECT) →
2) Worker/Hangfire → ტრანსფორმაცია JSON‑ად →
3) Elasticsearch Bulk API →
4) API → ძიება/აგრეგაცია →
5) Kibana → მონიტორინგი/ვალიდაცია

---

## Elasticsearch vs SQL – როდის რა გამოვიყენოთ
- SQL: ტრანზაქციები, სქემები, JOIN‑ები, მკაცრი კონსისტენტობა (ACID)
- Elasticsearch: JSON დოკუმენტები, schema‑on‑write, სწრაფი full‑text/ანალიტიკა, eventual consistency
- სიგნალი: თუ მთავარი ამოცანაა ძიების სიჩქარე/რელევანტობა და ტექსტის ანალიზი → Elasticsearch; რთული ბუღალტრული/ტრანზაქციული ლოგიკა → SQL

---

## საბაზისო კონცეფციები
- Index – დოკუმენტების ლოგიკური კონტეინერი
- Shard – ინდექსის ფიზიკური დაყოფა ჰორიზონტალური მასშტაბირებისთვის
- Replica – shard‑ის ასლები – მაღალი ხელმისაწვდომობა/კითხვის throughput
- Mapping – ველების ტიპები და ქცევა ძიებისას
- Analyzer/Tokenizer/Filter – ტექსტის დაშლა, ნორმალიზაცია, სინონიმები

---

## ინდექსის მეპინგი და ანალიზატორები (პრაქტიკული JSON)
```json
PUT products
{
  "settings": {
    "number_of_shards": 1,
    "number_of_replicas": 1,
    "refresh_interval": "1s",
    "analysis": {
      "analyzer": {
        "product_name_analyzer": {
          "type": "custom",
          "tokenizer": "standard",
          "filter": ["lowercase", "stop", "kstem"]
        },
        "autocomplete_analyzer": {
          "type": "custom",
          "tokenizer": "edge_ngram_tokenizer",
          "filter": ["lowercase"]
        }
      },
      "tokenizer": {
        "edge_ngram_tokenizer": {
          "type": "edge_ngram",
          "min_gram": 2,
          "max_gram": 20,
          "token_chars": ["letter", "digit"]
        }
      }
    }
  },
  "mappings": {
    "properties": {
      "id": { "type": "keyword" },
      "name": {
        "type": "text",
        "analyzer": "product_name_analyzer",
        "fields": {
          "keyword": { "type": "keyword" },
          "autocomplete": { "type": "text", "analyzer": "autocomplete_analyzer" }
        }
      },
      "description": { "type": "text", "analyzer": "standard" },
      "price": { "type": "float" },
      "stock": { "type": "integer" },
      "category": {
        "type": "keyword",
        "fields": { "text": { "type": "text" } }
      },
      "tags": { "type": "keyword" },
      "createdDate": { "type": "date" },
      "isActive": { "type": "boolean" },
      "specifications": {
        "type": "nested",
        "properties": {
          "brand": { "type": "keyword" },
          "model": { "type": "keyword" }
        }
      }
    }
  }
}
```

შენიშვნა: თუ შეცვლით ანალიზატორებს/ველების ტიპებს, შექმენით ახალი ინდექსი და გამოიყენეთ reindex.

---

## მონაცემების ფორმა: SQL vs Elasticsearch
SQL:

| Id | Name         | Price   | Category |
|----|--------------|---------|----------|
| 1  | MacBook Pro  | 4999.99 | Laptops  |

Elasticsearch დოკუმენტი:
```json
{
  "id": 1,
  "name": "MacBook Pro",
  "price": 4999.99,
  "category": "Laptops",
  "tags": ["apple", "laptop"],
  "specifications": { "brand": "Apple", "model": "MacBook Pro 2024" }
}
```

---

## მონაცემების სინქრონიზაცია SQL→Elasticsearch
- ამოღება: `SELECT` SQL‑დან
- ტრანსფორმაცია: ობიექტ → JSON
- ჩატვირთვა: `Bulk API` (batched, retries, backoff)
- რეკომენდაციები:
  - გამოიყენეთ idempotent ოპერაციები (`index`/`update` `doc_as_upsert:true`‑ით)
  - დააყენეთ `refresh_interval` > `1s` მასიური ჩატვირთვისას; დასრულების შემდეგ `POST /products/_refresh`
  - კონტროლირებადი batch ზომა (მაგ. 2–5MB per request)

---

## API მაგალითები (პროექტიდან)
- Complex Bool Search
```
GET /api/advancedsearch/complex?query=macbook&category=laptops&maxPrice=3000&page=1&pageSize=20
```
- Fuzzy Search
```
GET /api/advancedsearch/fuzzy?query=mackbok&maxEdits=2
```
- Autocomplete
```
GET /api/advancedsearch/autocomplete?prefix=mac&limit=10
```
- Paginated Search
```
GET /api/advancedsearch/paginated1?query=macbook&page=1&pageSize=10
```

---

## ძიების ლოგიკა (კონცეპტი)
- MUST: `multi_match` (name, description, category.text)
- FILTER: `category` (keyword), `price <= X`, `isActive=true`
- SHOULD: `tags:featured` boost, `stock>0` boost
- Sort: `_score` ↓, `createdDate` ↓
- Highlight: `name`, `description`

---

## Kibana – სწრაფი ვალიდაცია
- Discover – დოკუმენტების ინსპექტირება
- Console – კითხვის ტესტირება (`_search`, `aggregations`)
- Visualize – კატეგორიების/ფასების განაწილება

მაგალითი – Terms Aggregation კატეგორიებზე:
```json
GET products/_search
{
  "size": 0,
  "aggs": {
    "by_category": {
      "terms": { "field": "category" }
    }
  }
}
```
შედეგი (მაგ.): `{ "Laptops": 15, "Phones": 8 }`

---

## Docker Compose – გაშვება
- Volumes: `elasticsearch-data`, `sqlserver-data`
- Network: `elastic-network`
- სერვისების გაშვება:
```
docker compose up -d
```
- ლოგები:
```
docker compose logs -f elasticsearch
```

---

## Reindex – საუკეთესო პრაქტიკები
1) შექმენით ახალი ინდექსი ახალი mapping/settings‑ით
2) გაწერეთ `aliases` ცვლილებებისთვის (`products_v2` ↔ `products_current`)
3) გამოიყენეთ `_reindex` მხოლოდ დოკუმენტების გადასატანად; მეპინგები/ანალიზატორები ხელით შექმენით წინასწარ
4) დააკვირდით ველების rename/ტიპების ცვლილებებს – საჭიროებისას `script` reindex-ში

---

## Refresh Interval – ტიუნინგი
- მოკლე (მაგ. `1s`): სწრაფი ქვეინდექსაცია, მეტი I/O
- გრძელი (მაგ. `30s` ან `-1` დროებით): bulk ჩატვირთვისას უკეთესი throughput
- წესი: ინდესქის heavily write ფაზაში გაზარდეთ, კითხვითი ფაზაში შეამცირეთ

---

## Keyword vs Text – სწორი არჩევანი
- `keyword` – ზუსტი შესაბამისობა, ფილტრი/სორტი/აგრეგაცია
- `text` – სრული ტექსტის ძიება, ანალიზატორებით; არ გამოიყენება აგრეგაციაში (გარდა `fields.keyword`)

---

## Autocomplete – edge_ngram პრაქტიკაში
- `edge_ngram` tokenizer აყალიბებს პრეფიქსულ ტოკენებს (`ma`, `mac`, `macb` ...)
- მოთხოვნა: `match`/`prefix` `name.autocomplete` ველზე
- შედეგი: მომენტალური ავტოშევსება UI‑ში

---

## Hangfire/Worker – საიმედო სინქრონიზაცია
- განმეორებადი Jobs: Retry/backoff, dead‑letter სტრატეგია
- Concurrency კონტროლი: Queue per entity, Locking
- ტრანზაქციულობა: SQL → Outbox Pattern (სასურველია)

---

## წარმადობა, სტაბილურობა, უსაფრთხოება
- Performance: სწორი shard რაოდენობა (საწყისად 1–3), batch bulk, `_source` selective fields თუ საჭიროა
- Stability: replicas ≥1 production-ში, node monitoring, ILM (მოცულობის მართვა)
- Security: TLS, API key/Basic auth, როლებზე დაფუძნებული წვდომა (Kibana Spaces)

---

## Troubleshooting – ხშირად დასმული პრობლემები
- „ძებნა გვიან ხედავს ახალ დოკებს“ → `refresh_interval`/`_refresh`
- „აგრეგაციები ნელა მუშაობს“ → ველების keyword‑იზაცია, `shard_size`/`size` ტუნინგი
- „მიჩნეულია, მაგრამ ვერ პოულობს“ → ანალიზატორი/ტოკენიზატორი, `explain:true` ტესტირება

---

## Go‑Live ჩეკლისტი
- [ ] Mapping/Settings დამტკიცებულია და version‑ირებულია (IaC)
- [ ] Index Alias სტრატეგია მზადაა (zero‑downtime deploy)
- [ ] მონიტორინგი/Kibana Dashboard‑ები კონფიგურირებულია
- [ ] Backup/Snapshot პოლიტიკა ჩართულია

---

## ტერმინების ლექსიკონი
- Shard – ინდექსის ფრაგმენტი ჰორიზონტალური მასშტაბირებისთვის
- Replica – shard‑ის ასლი მაღალი ხელმისაწვდომობისთვის
- Analyzer – ტექსტის დაშლის/ნორმალიზაციის მექანიზმი

---

## Q&A
დამატებითი კითხვები ან დეტალები? სიამოვნებით განვიხილავ კონკრეტულ ქეისებს, ინდექსის დიზაინს ან შესრულების ტიუნინგს.
