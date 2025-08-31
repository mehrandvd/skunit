# SCENARIO JSON User Info Response

## [USER]
Give me user info as JSON

## [AGENT]
{"name": "John", "age": 30, "city": "New York"}

### CHECK JsonCheck
{
  "name": ["NotEmpty"],
  "age": ["GreaterThan", 0],
  "city": ["SemanticCondition", "It's a real city name"]
}