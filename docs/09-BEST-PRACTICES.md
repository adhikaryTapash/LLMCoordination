# Best Practices

## Multi-Tenant Safety

- Every table must include tenant_id where data is tenant-specific.
- Never trust tenant_id from frontend blindly.
- Resolve tenant from authenticated user/session.
- Enforce tenant filter in every query.
- Use row-level security later if needed.

## Credential Security

- Never store raw API keys or DB passwords.
- Encrypt credentials before saving.
- Use Azure Key Vault in production.
- Never send credentials to OpenAI.
- LLM should receive only metadata, not secrets.

## Skill Design

- Keep global skills reusable.
- Do not create skill per endpoint.
- Store tenant endpoint mapping separately.
- Use semantic search to match user question to skill and endpoint.
- Use approval for update/delete/create actions.

## OpenAI Usage

- Use OpenAI for reasoning, intent, planning, and response composition.
- Do not allow OpenAI to directly execute tools.
- Backend must execute all API/MCP/DB calls.
- Always validate tool parameters.
- Always log tool execution.

## SQL Safety

- Start with read-only queries.
- Block DELETE, UPDATE, INSERT, DROP, ALTER.
- Enforce LIMIT.
- Enforce tenant filter.
- Validate generated SQL before execution.

## API Execution Safety

- GET requests can run with permission.
- POST/PATCH/DELETE should need approval initially.
- Validate required parameters before execution.
- Mask sensitive data in response.

## Frontend Best Practices

- Render based on response viewType.
- Do not hardcode domain-specific UI.
- Use generic CardResultView and TableResultView.
- Keep chat response schema consistent.

## Audit

Log every:

- User question
- Intent
- Skill
- Selected endpoint/tool
- Request payload
- Response summary
- Token cost
- Execution time
- User approval
- Error

