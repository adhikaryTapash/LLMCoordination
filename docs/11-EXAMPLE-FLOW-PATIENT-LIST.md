# Example Flow - Patient List Card View

## User Question

```text
Give me the list of patients in card view
```

## NLU Output

```json
{
  "intent": {
    "name": "patient.list",
    "confidence": 0.96,
    "domain": "healthcare",
    "resource": "patient",
    "action": "read"
  },
  "entities": {
    "operation": "list",
    "viewType": "card",
    "limit": 20
  }
}
```

## Skill Router Output

```json
{
  "selectedSkill": "api.query.execute",
  "reason": "User requested a list/read operation that should map to a GET endpoint."
}
```

## Endpoint Discovery Output

```json
{
  "selectedEndpoint": {
    "method": "GET",
    "path": "/api/patients",
    "operationId": "patient_list",
    "confidence": 0.94
  }
}
```

## Final Chat Response

```json
{
  "status": "success",
  "viewType": "card",
  "answer": "Here is the list of patients.",
  "data": {
    "totalRecords": 3,
    "cards": [
      {
        "title": "Rahul Sharma",
        "subtitle": "MRN: P-1001",
        "fields": [
          { "label": "Age", "value": "42" },
          { "label": "Gender", "value": "Male" },
          { "label": "Status", "value": "Active" }
        ]
      }
    ]
  }
}
```

