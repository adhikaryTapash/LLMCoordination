# PostgreSQL Database Design

## Multi-Tenant Tables

### tenants
Stores tenant/company details.

Columns:
- id
- name
- domain_type: healthcare, erp, mixed
- status
- created_at

### users
Stores application users.

Columns:
- id
- tenant_id
- full_name
- email
- password_hash
- role
- status
- created_at

### tenant_swagger_documents
Stores uploaded Swagger documents.

Columns:
- id
- tenant_id
- name
- document_url
- raw_json
- version
- status
- created_at

### tenant_swagger_endpoints
Stores parsed Swagger endpoints.

Columns:
- id
- tenant_id
- swagger_document_id
- method
- path
- operation_id
- summary
- description
- request_schema_json
- response_schema_json
- auth_required
- tags
- embedding_vector
- created_at

### tenant_mcp_servers
Stores MCP server config.

Columns:
- id
- tenant_id
- name
- base_url
- encrypted_credentials
- status
- created_at

### tenant_mcp_tools
Stores MCP tool metadata.

Columns:
- id
- tenant_id
- mcp_server_id
- tool_name
- description
- input_schema_json
- output_schema_json
- embedding_vector
- created_at

### tenant_database_connections
Stores encrypted DB connection details.

Columns:
- id
- tenant_id
- db_type
- host
- database_name
- encrypted_connection_string
- read_only
- status
- created_at

### tenant_database_schema
Stores discovered database metadata.

Columns:
- id
- tenant_id
- connection_id
- table_name
- column_name
- data_type
- is_nullable
- relationship_info
- embedding_vector
- created_at

## Global Skill Tables

### global_skill_master
Reusable skills shared by all tenants.

Columns:
- id
- skill_name
- domain
- category
- description
- action_type
- enabled
- created_at

### global_skill_intent_examples
Intent examples for semantic matching.

Columns:
- id
- skill_id
- example_text
- embedding_vector

### global_skill_prompt
Prompt template for skill execution.

Columns:
- id
- skill_id
- prompt_template
- version
- active

### global_skill_response_schema
Expected output format.

Columns:
- id
- skill_id
- response_schema_json
- version
- active

## Tenant Mapping Tables

### tenant_skill_endpoint_mapping
Maps global skills to tenant endpoints.

Columns:
- id
- tenant_id
- skill_id
- endpoint_id
- confidence_score
- enabled

### tenant_skill_mcp_mapping
Maps global skills to tenant MCP tools.

Columns:
- id
- tenant_id
- skill_id
- mcp_tool_id
- confidence_score
- enabled

### tenant_permission_rules
Stores tenant/user permissions.

Columns:
- id
- tenant_id
- role
- resource
- action
- allowed

## Chat & Audit Tables

### conversations
Columns:
- id
- tenant_id
- user_id
- title
- created_at

### conversation_messages
Columns:
- id
- conversation_id
- role
- message_text
- response_json
- created_at

### agent_audit_logs
Columns:
- id
- tenant_id
- user_id
- conversation_id
- turn_id
- agent_name
- skill_name
- tool_type
- tool_name
- request_json
- response_json
- status
- cost
- execution_ms
- created_at

