# Elasticsearch პროექტის პრეზენტაცია

---

## პროექტის მიზანი
- SQL Server-დან მონაცემების სინქრონიზაცია Elasticsearch-ში
- სწრაფი და მოქნილი ძიება, ავტოშევსება, აგრეგაცია
- Docker-ით მარტივი გაშვება და ინფრასტრუქტურის მართვა

---

## არქიტექტურა
- **SQL Server**: მონაცემების წყარო
- **Elasticsearch**: ძიების და ანალიტიკის სისტემა
- **Hangfire Jobs**: ფონური სინქრონიზაციის პროცესები
- **API**: მომხმარებლისთვის ძიების და მონაცემების წვდომა
- **Docker Compose**: სერვისების გაერთიანებული გაშვება

---

## Elasticsearch - NoSQL და განსხვავება SQL-თან
- **NoSQL**: დოკუმენტური ბაზა, ინახავს JSON დოკუმენტებს, schema-less, არ აქვს ცხრილები და JOIN-ები
- **SQL**: სტრუქტურირებული ცხრილები, სქემა, JOIN-ები, ACID პრინციპები
- **Elasticsearch**: სწრაფი full-text ძიება, eventual consistency, REST API, horizontal scaling

---

## Elasticsearch-ის გამორჩეული თვისებები
- Full-text ძიება (ტექსტის დაშლა, ანალიზი, სინონიმები)
- Aggregations (სტატისტიკა, ჯგუფირება)
- Autocomplete (edge_ngram)
- Horizontal scaling (shards)
- High availability (replicas)
- REST API
- Schema-less

---

## აუცილებელი პარამეტრები
- **NumberOfShards**: ინდექსის დაყოფა shard-ებად, პარალელური ძიება და მასშტაბირება
- **NumberOfReplicas**: თითო shard-ის backup-ები, მონაცემების ხელმისაწვდომობა
- **RefreshInterval**: ახალი დოკუმენტების ძიებისთვის ხელმისაწვდომობის სიხშირე
- **Analysis/Tokenizers/Filters**: ტექსტის დაშლის და ძიების ოპტიმიზაცია

---

## მონაცემების ფორმა: SQL vs Elasticsearch
- **SQL ბაზაში**: ცხრილები, სტრიქონები, რიცხვები, თარიღები
- **Elasticsearch-ში**: JSON დოკუმენტები, nested ობიექტები, მასივები

### მაგალითი:
SQL:
| Id | Name           | Price   | Category  |
|----|---------------|---------|-----------|
| 1  | MacBook Pro   | 4999.99 | Laptops   |

Elasticsearch:
{
  "id": 1,
  "name": "MacBook Pro",
  "price": 4999.99,
  "category": "Laptops",
  "tags": ["apple", "laptop"],
  "specifications": { "brand": "Apple", "model": "MacBook Pro 2024" }
}

---

## მონაცემების გადატანა ბაზიდან Elasticsearch-ში
- მონაცემები იკითხება SQL-დან (SELECT ...)
- გარდაიქმნება JSON ფორმატში
- იგზავნება Elasticsearch-ში (Bulk API)

---

## Kibana-ში გადამოწმება
- **Discover**: ინდექსის დოკუმენტების ნახვა
- **Aggregations/Visualizations**: კატეგორიების სტატისტიკა, ფასების განაწილება
- **Query**: full-text ძიება, ფილტრები, autocomplete

---

## Elasticsearch-ის ინდექსის მეპინგი
- **Id**: keyword (ზუსტი შესაბამისობა)
- **Name**: text (search), keyword (sort/aggregate), autocomplete (edge_ngram)
- **Description**: text (search)
- **Price**: float
- **Stock**: integer
- **Category**: keyword (filter), text (search)
- **Tags**: keyword array
- **CreatedDate**: date
- **IsActive**: boolean
- **Specifications**: nested object (brand/model)

---

## ანალიზატორები და ტოკენიზატორები
- **product_name_analyzer**: lowercase, stopwords, stemming, synonyms
- **autocomplete_analyzer**: edge_ngram tokenizer, lowercase
- **edge_ngram_tokenizer**: MinGram(2), MaxGram(20), letters/digits

---

## ძიების ლოგიკა
- **ComplexBoolSearchAsync**:
  - MUST: MultiMatch (Name, Description, Category)
  - FILTER: Category, Price, IsActive
  - SHOULD: featured tag, stock > 0
  - Highlight: Name/Description
  - Sort: Score, CreatedDate

### მაგალითი
- query: "ლეპტოპი", category: "ლეპტოპები", maxPrice: 2000
- Product A: გაივლის ყველა ფილტრს, featured tag-ით ქულა მაღალი
- Product B: ვერ გადის category ფილტრს

---

## აგრეგაციები
- Terms Aggregation: კატეგორიების მიხედვით პროდუქტების რაოდენობა
- Example: { "Laptops": 15, "Phones": 8 }

---

## Docker Compose
- **volumes**: elasticsearch-data, sqlserver-data (მონაცემების შენახვა)
- **networks**: elastic-network (სერვისების დაკავშირება)

---

## Reindex
- მონაცემების გადატანა ერთი ინდექსიდან მეორეში
- მხოლოდ დოკუმენტები გადადის, მეპინგები/ანალიზატორები ხელით უნდა შეიქმნას

---

## Refresh Interval
- განსაზღვრავს, რამდენად ხშირად ხდება ახალი დოკუმენტების ძიებისთვის ხელმისაწვდომობა
- მოკლე ინტერვალი: სწრაფი ძიება, მეტი დატვირთვა
- გრძელი ინტერვალი: ნაკლები დატვირთვა, გვიანი ძიება

---

## Keyword vs Text
- **keyword**: ზუსტი შესაბამისობა, სორტირება, აგრეგაცია
- **text**: სრული ტექსტის ძიება, ანალიზატორით

---

## Autocomplete
- edge_ngram tokenizer: ტექსტის დაყოფა n-გრამებად
- ავტოშევსების ფუნქციონალი

---

## Hangfire და Worker
- Jobs მუდმივად მუშაობს, ფონური სინქრონიზაცია
- Deadlock-ების და concurrency-ის მართვა

---

## დასკვნა
- Elasticsearch-ის მორგებული მეპინგები და ანალიზატორები მნიშვნელოვნად აუმჯობესებს ძიების შედეგებს
- Docker-ით მარტივი გაშვება და ინფრასტრუქტურის მართვა
- Hangfire Jobs უზრუნველყოფს მონაცემების მუდმივ სინქრონიზაციას

---

**დამატებითი კითხვები ან დეტალები?**
