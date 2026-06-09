# API Contracts

## Chat Request

```json
{
  "conversationId": "conv_1001",
  "message": "Give me the list of patients in card view",
  "viewPreference": "card"
}
```

## Chat Response

```json
{
  "conversationId": "conv_1001",
  "messageId": "msg_2001",
  "intent": "patient.list",
  "skill": "api.query.execute",
  "viewType": "card",
  "status": "success",
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
          { "label": "Mobile", "value": "9876543210" }
        ],
        "actions": [
          { "label": "View Profile", "action": "open_patient_profile", "entityId": "1001" }
        ]
      }
    ]
  }
}
```

## NLU Agent Request

```json
{
  "conversationId": "conv_1001",
  "turnId": "turn_5001",
  "tenantId": "tenant_001",
  "userId": "user_101",
  "userRole": "Admin",
  "rawMessage": "Give me the list of upcoming appointments for the patient XXX",
  "channel": "web_chat",
  "context": {
    "availableDomains": ["healthcare", "erp", "swagger", "mcp", "database"]
  }
}
```

## NLU Agent Response

```json
{
  "agentId": "nlu.intent",
  "status": "success",
  "intent": {
    "name": "appointment.list.upcoming",
    "confidence": 0.97,
    "domain": "healthcare",
    "resource": "appointment",
    "action": "read"
  },
  "entities": {
    "resource": "appointments",
    "operation": "list",
    "timeScope": "upcoming",
    "patient": {
      "identifierType": "name",
      "identifierValue": "XXX"
    },
    "filters": {
      "appointmentDate": {
        "operator": ">=",
        "value": "current_date"
      }
    },
    "sort": {
      "field": "appointmentDate",
      "direction": "asc"
    },
    "limit": 20,
    "viewType": "list"
  },
  "routingHints": {
    "requiresPatientResolution": true,
    "requiresDatabase": false,
    "requiresSwagger": true,
    "requiresMcpTool": false,
    "suggestedSkill": "appointment.upcoming.byPatient",
    "suggestedNextAgent": "tenant.context.agent"
  },
  "clarification": {
    "required": false,
    "question": null
  }
}
```

## Swagger Registration Request

```json
{
  "name": "Healthcare API",
  "swaggerUrl": "https://example.com/swagger/v1/swagger.json",
  "domain": "healthcare"
}
```

## MCP Registration Request

```json
{
  "name": "Healthcare MCP Server",
  "baseUrl": "https://mcp.example.com",
  "credentialType": "bearer_token",
  "credentialValue": "encrypted-by-backend"
}
```

