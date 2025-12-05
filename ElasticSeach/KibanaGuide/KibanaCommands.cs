namespace ElasticSeach.KibanaGuide;

/// <summary>
/// Kibana Dev Tools Console Commands
/// Copy-paste these commands into Kibana Dev Tools (http://localhost:5601)
/// </summary>
public static class KibanaCommands
{
    public const string Guide = @"
# ==========================================
# KIBANA BASICS - DEV TOOLS CONSOLE GUIDE
# ==========================================

# 7. Kibana Dev Tools - Basic Commands
# Open Kibana: http://localhost:5601
# Navigate to: Management > Dev Tools

# ==========================================
# CLUSTER AND INDEX MANAGEMENT
# ==========================================

# Check cluster health
GET _cluster/health

# List all indices
GET _cat/indices?v

# Get specific index information
GET products

# Get index mappings
GET products/_mapping

# Get index settings
GET products/_settings

# ==========================================
# DOCUMENT OPERATIONS
# ==========================================

# Index a single document
PUT products/_doc/100
{
  ""name"": ""MacBook Pro 16"",
  ""description"": ""Professional laptop with M3 Max chip"",
  ""price"": 2499.99,
  ""stock"": 15,
  ""category"": ""Electronics"",
  ""tags"": [""laptop"", ""apple"", ""professional""],
  ""isActive"": true
}

# Get a document by ID
GET products/_doc/1

# Update a document
POST products/_update/1
{
  ""doc"": {
    ""price"": 1299.99,
    ""stock"": 18
  }
}

# Delete a document
DELETE products/_doc/100

# ==========================================
# SEARCH QUERIES
# ==========================================

# Match all documents
GET products/_search
{
  ""query"": {
    ""match_all"": {}
  }
}

# Match query (full-text search)
GET products/_search
{
  ""query"": {
    ""match"": {
      ""name"": ""laptop dell""
    }
  }
}

# Multi-match query
GET products/_search
{
  ""query"": {
    ""multi_match"": {
      ""query"": ""smartphone camera"",
      ""fields"": [""name^2"", ""description""]
    }
  }
}

# Term query (exact match)
GET products/_search
{
  ""query"": {
    ""term"": {
      ""category"": ""Electronics""
    }
  }
}

# Range query
GET products/_search
{
  ""query"": {
    ""range"": {
      ""price"": {
        ""gte"": 500,
        ""lte"": 1500
      }
    }
  }
}

# Bool query (combining queries)
GET products/_search
{
  ""query"": {
    ""bool"": {
      ""must"": [
        { ""term"": { ""category"": ""Electronics"" } }
      ],
      ""filter"": [
        { ""range"": { ""price"": { ""lt"": 1000 } } }
      ],
      ""should"": [
        { ""match"": { ""name"": ""Apple"" } },
        { ""match"": { ""name"": ""Samsung"" } }
      ],
      ""minimum_should_match"": 1
    }
  }
}

# ==========================================
# AGGREGATIONS
# ==========================================

# Terms aggregation (grouping)
GET products/_search
{
  ""size"": 0,
  ""aggs"": {
    ""categories"": {
      ""terms"": {
        ""field"": ""category""
      }
    }
  }
}

# Stats aggregation
GET products/_search
{
  ""size"": 0,
  ""aggs"": {
    ""price_stats"": {
      ""stats"": {
        ""field"": ""price""
      }
    }
  }
}

# Histogram aggregation
GET products/_search
{
  ""size"": 0,
  ""aggs"": {
    ""price_ranges"": {
      ""histogram"": {
        ""field"": ""price"",
        ""interval"": 500
      }
    }
  }
}

# ==========================================
# TEXT ANALYSIS
# ==========================================

# Analyze text with standard analyzer
GET products/_analyze
{
  ""analyzer"": ""standard"",
  ""text"": ""The Quick Brown Foxes are jumping!""
}

# Analyze text with custom analyzer
GET products/_analyze
{
  ""analyzer"": ""product_analyzer"",
  ""text"": ""High-Performance Laptop""
}

# Analyze with specific tokenizer and filters
GET _analyze
{
  ""tokenizer"": ""standard"",
  ""filter"": [""lowercase"", ""stop""],
  ""text"": ""The quick brown fox jumps""
}

# ==========================================
# INDEX OPERATIONS
# ==========================================

# Create index with settings
PUT test_index
{
  ""settings"": {
    ""number_of_shards"": 1,
    ""number_of_replicas"": 0
  },
  ""mappings"": {
    ""properties"": {
      ""name"": { ""type"": ""text"" },
      ""age"": { ""type"": ""integer"" },
      ""email"": { ""type"": ""keyword"" }
    }
  }
}

# Delete index
DELETE test_index

# Reindex data
POST _reindex
{
  ""source"": {
    ""index"": ""products""
  },
  ""dest"": {
    ""index"": ""products_backup""
  }
}

# ==========================================
# BULK OPERATIONS
# ==========================================

# Bulk indexing
POST _bulk
{ ""index"": { ""_index"": ""products"", ""_id"": ""101"" } }
{ ""name"": ""Product 1"", ""price"": 99.99 }
{ ""index"": { ""_index"": ""products"", ""_id"": ""102"" } }
{ ""name"": ""Product 2"", ""price"": 149.99 }

# ==========================================
# SORTING AND PAGINATION
# ==========================================

# Sort by price descending
GET products/_search
{
  ""query"": { ""match_all"": {} },
  ""sort"": [
    { ""price"": ""desc"" }
  ]
}

# Pagination
GET products/_search
{
  ""from"": 0,
  ""size"": 3,
  ""query"": { ""match_all"": {} }
}

# ==========================================
# HIGHLIGHTING
# ==========================================

# Highlight search terms
GET articles/_search
{
  ""query"": {
    ""match"": {
      ""content"": ""elasticsearch query""
    }
  },
  ""highlight"": {
    ""fields"": {
      ""content"": {}
    }
  }
}

# ==========================================
# ADVANCED QUERIES
# ==========================================

# Wildcard query
GET products/_search
{
  ""query"": {
    ""wildcard"": {
      ""name.keyword"": ""*phone*""
    }
  }
}

# Fuzzy query (typo tolerance)
GET products/_search
{
  ""query"": {
    ""fuzzy"": {
      ""name"": {
        ""value"": ""labtop"",
        ""fuzziness"": ""AUTO""
      }
    }
  }
}

# Prefix query
GET products/_search
{
  ""query"": {
    ""prefix"": {
      ""name.keyword"": ""iPad""
    }
  }
}

# Exists query
GET products/_search
{
  ""query"": {
    ""exists"": {
      ""field"": ""specifications""
    }
  }
}

# ==========================================
# KIBANA DISCOVER
# ==========================================

# After creating indices and indexing data:
# 1. Go to: Management > Stack Management > Index Patterns
# 2. Create index pattern: products*
# 3. Select timestamp field: createdDate (or @timestamp)
# 4. Go to: Analytics > Discover
# 5. Select your index pattern
# 6. Use KQL (Kibana Query Language):
#    - category: ""Electronics""
#    - price >= 500 and price <= 1500
#    - name: *phone*
#    - tags: (laptop OR tablet)

# ==========================================
# USEFUL COMMANDS
# ==========================================

# Count documents
GET products/_count

# Refresh index
POST products/_refresh

# Clear cache
POST products/_cache/clear

# Force merge
POST products/_forcemerge

# Get field mapping
GET products/_mapping/field/name

";
}

