# 🚀 Elasticsearch სრული პრეზენტაცია (ქართული)

ეს პრეზენტაცია ფარავს Elasticsearch-ის არქიტექტურას, API‑ებს, ინდექსირების/ძებნის საფუძვლებსა და მოწინავე ფუნქციონალებს, ასევე Kibana‑ს, Logstash‑ს და HA/კლასტერიზაციას.

---

## 🏗️ 1. Elasticsearch არქიტექტურა და ფუნდამენტები
Elasticsearch არის დისტრიბუტირებული, RESTful საძიებო და ანალიტიკური სისტემა.
- **Cluster (კლასტერი):** ნოდების (სერვერების) ერთობლიობა, რომელიც ინახავს მთელ მონაცემებს.
- **Node (ნოდი):** ერთეული სერვერი კლასტერში, რომელიც ინახავს მონაცემს და ასრულებს ინდექსაციას/ძებნას.
- **Index (ინდექსი):** დოკუმენტების კოლექცია (შედარებითი ანალოგი — „ბაზა“ RDBMS-ში).
- **Document (დოკუმენტი):** JSON ობიექტი — ინფორმაციის საბაზო ერთეული.
- **Inverted Index (ინვერტირებული ინდექსი):** ძირითადი სტრუქტურა სწრაფი ძებნისთვის (ტერმინი → დოკუმენტების სია).
- **Shards (შარდები):** ინდექსის ლოგიკური დაყოფა. ყოველი შარდი დამოუკიდებელი „მინი ინდექსია“.
- **Replicas (რეპლიკები):** შარდების ასლები — მაღალი ხელმისაწვდომობისა და სწრაფი ძებნისთვის.

---

## 📡 2. Elasticsearch API
REST API ყველა ძირითად ოპერაციაზე:
- **Document APIs:** Index/Get/Update/Delete (`POST /index/_doc`).
- **Search APIs:** ძებნა Query DSL‑ით (`GET /index/_search`).
- **Indices APIs:** ინდექსების, Mapping/Settings მართვა (`PUT /index`).
- **Cluster APIs:** კლასტერის ჯანმრთელობა/სტატუსი (`GET /_cluster/health`).

---

## 🛠️ 3. ინდექსის შექმნა და Template‑ები
- **Index creation:** შარდები/რეპლიკები/ანალიზი/მეპინგები.
- **Index Templates:** წინასწარი წესები, რომლებიც ავტომატურად ვრცელდება ახალი ინდექსებზე (მაგ. `products-*`, `logs-*`).
```json
PUT /_index_template/products_template
{
  "index_patterns": ["products-*"],
  "template": {
    "settings": { "number_of_shards": 1, "number_of_replicas": 1 },
    "mappings": { "properties": { "category": { "type": "keyword" } } }
  }
}
```

---

## 📐 4. მონაცემთა ტიპები და Mappings
Mapping განსაზღვრავს ველების ტიპს და ინდექსაციის წესს.
- **ძირითადი ტიპები:** `text`, `keyword`, `long`, `double`, `boolean`, `date`.
- **კომპოზიტური:** `object`, `nested` (ქვე‑ობიექტების ურთიერთობის შენარჩუნებით).
- **სპეციალური:** `geo_point`, `ip`, `completion`.

---

## 📥 5. დოკუმენტების ინდექსაცია
- **ერთი დოკუმენტი:** `POST /products/_doc/1 { "name": "Laptop" }`
- **Bulk API:** მასობრივი ოპერაციები ერთ მოთხოვნაში (მაღალი წარმადობა).

---

## 🔍 6. ტექსტის ანალიზის საფუძვლები
`text` ველები გადიან **Analyzer**‑ს:
- **Character Filters:** ტექსტის გასუფთავება ( напр. HTML strip ).
- **Tokenizer:** ტექსტის დაჭრა ტოკენებად.
- **Token Filters:** ტოკენების ტრანსფორმაცია (lowercase, stop, stemming, synonyms).

---

## 🔎 7. Data search და Query DSL საფუძვლები
JSON‑ზე დაფუძნებული ენა ძიებისთვის:
- **Leaf Queries:** `match`, `term`, `range`.
- **Compound Queries:** `bool` (must, filter, should, must_not).

---

## 🖥️ 8. Kibana — Dev Tools, Discover
Kibana არის Elastic Stack‑ის UI.
- **Discover:** დოკუმენტების დათვალიერება/გაფილტვრა.
- **Dev Tools (Console):** Raw REST მოთხოვნები autocomplete‑ით.
- **Visualize & Dashboards:** გრაფიკები და ინტერაქტიული დაფები.

---

## 🚀 9. Elasticsearch.Net და NEST .NET პროექტებში
- **NEST:** ძლიერი, ტიპიზირებული კლიენტი Fluent DSL‑ით, რომელიც Query DSL‑ს შეესაბამება.
- **Elasticsearch.Net:** დაბალდონიანი, მინიმალისტური კლიენტი HTTP კომუნიკაციით.
- **მაგალითი:**
```csharp
var resp = await client.SearchAsync<Product>(s => s
    .Query(q => q.Match(m => m.Field(f => f.Name).Query("laptop")))
);
```

---

## 🧪 10. Advanced Text Analysis — Analyzers, Tokenizers, Filters, Normalizers
- **Analyzers:** Character Filters + Tokenizer + Token Filters (კონფიგურირებადი კომბო).
- **Tokenizers:** `standard`, `whitespace`, `edge_ngram` (autocomplete‑ისთვის).
- **Token Filters:** `lowercase`, `stop`, `snowball`, `synonym` და სხვ.
- **Character Filters:** `html_strip`, `mapping` და სხვ.
- **Normalizers:** მხოლოდ `keyword` ველებისთვის (Tokenizers არ აქვს; ერთი ტოკენი).

---

## 🔎 11. Advanced Data search და Query DSL
- **Fuzzy Search:** typo‑ების ტოლერანტული ძებნა.
- **Multi‑match:** ერთდროულად რამდენიმე ველში ძებნა.
- **Highlighting:** დამთხვევების მონიშვნა.
- **Bool Queries:** Must/Filter/Should სტრატეგიებით რელევანტობა და ფილტრაცია.

---

## 🧭 12. Aliases (ალიასები)
- ინდექს(ებ)ის ზედმეტსახელი — აუცილებელია **Zero‑Downtime Reindexing**‑ისთვის.
- ერთ ბრძანებით გადართავთ აპს `products_v1`‑დან `products_v2`‑ზე.

---

## 📊 13. Kibana Dashboards
- აგრეგაციებზე დაფუძნებული ვიზუალიზაციები (ბარათები/პანელები).
- დროში სერიების გრაფები, კატეგორიების განხეთქილებები, KPI‑ები.

---

## 📈 14. Aggregations (Bucket, Metrics, Pipeline)
- **Bucket:** ჯგუფები ( напр. `terms` კატეგორიით, `range` ფასით ).
- **Metrics:** გამოთვლები ბაკეტებში ( `avg`, `sum`, `min`, `max`, `cardinality` ).
- **Pipeline:** აგრეგაციებზე აგრეგაციები ( напр. cumulative sum ).

---

## 🧱 15. Advanced Index Management — Reindex / Split / Clone / Merge / Shrink
- **Reindex:** მონაცემების გადატანა ერთი ინდექსიდან მეორეში (mapping ცვლილებებზე).
- **Split:** primary shard‑ების რაოდენობის გაზრდა.
- **Shrink:** primary shard‑ების შემცირება.
- **Clone:** ინდექსის ზუსტი ასლი.
- **Force Merge (Merge):** სეგმენტების შემცირება ოპტიმიზაციისთვის.

---

## 📜 16. Elasticsearch Scripting (Painless)
- **Use‑cases:** `script_score` (რანჟირება), `script_fields` (გამოთვლადი ველები), `update_by_query` (მასობრივი განახლება).
- **ენა:** Painless — უსაფრთხო და სწრაფი, Java‑ს მსგავსი სინტაქსით.

---

## 🚚 17. Logstash (pipelines და plugins: input, output, filter, codec)
- **Pipeline:** Input → Filter → Output.
- **Input:** `jdbc`, `file`, `beats`, `http`.
- **Filter:** `grok`, `mutate`, `date`, `geoip` და სხვ.
- **Output:** `elasticsearch`, `file`, `stdout`, `kafka`.
- **Codec:** `json`, `multiline` და სხვ.

---

## ⏳ 18. Rolling up historical data
- ძველი დეტალური მონაცემების შეკუმშვა დაბალი გრუნულაციით (საათური/დღიური) — დისკის ეკონომია და ტრენდების შენარჩუნება.

---

## 🔄 19. Transforming data (Transforms)
- ტრანზაქციებიდან/ლოგებიდან „entity‑centric“ ინდექსების აგება ( напр. `customer_summary`).

---

## 🌐 20. Clustering და High Availability (HA)
- **Master Nodes:** მართავს cluster state‑ს.
- **Data Nodes:** ინახავს/ძებნის მონაცემს.
- **Replicas:** ყოველთვის სხვა ნოდზე დგება, ვიდრე მისი Primary — HA‑სთვის.
- **Split‑brain დაცვა:** Quorum‑ზე დაფუძნებული ლიდერის არჩევა.

---

## 🌊 21. Data Streams
- Append‑only დროის სერიების მართვა ერთიანი სახელით (ქვეშ მრავალი ინდექსი) — იდეალურია ლოგებისა და მეტრიკებისთვის.

---

## 📊 22. Kibana Metrics
- Metricbeat‑ით ინფრასტრუქტურის მეტრიკების აღება (CPU, RAM, Disk, Network).

---

## 🛡️ 23. Kibana Monitoring, Alerting და Actions
- **Monitoring:** Stack Monitoring — კლასტერი/ნოდები/ინდექსაცია/ძებნის მაჩვენებლები.
- **Alerting:** წესები ( напр. CPU > 90% ) და მოქმედებები (Email, Slack, Webhook).
- **Actions:** ავტომატური რეაგირება ზღვრის გადაჭარბებაზე.

---

## 💡 რეკომენდაციები და საუკეთესო პრაქტიკა
- `keyword` ველები — ზუსტი ფილტრები/აგრეგაციები; `text` — full‑text ძებნა.
- **Index Templates** — სტანდარტიზაცია ახალი ინდექსებისთვის.
- **Normalizers** — Case‑insensitive keyword ველებისთვის.
- **Aliases** — გამოიყენეთ აპში ყოველთვის ალიასი zero‑downtime მიგრაციებისთვის.
- RAM‑ის ნახევარი დაუტოვეთ OS cache‑ს სწრაფი ძებნისთვის.
- Painless გამოიყენეთ, როცა წინასწარ დათვლა შეუძლებელია ( напр. დისტანცია, დინამიკური ქულირება ).
