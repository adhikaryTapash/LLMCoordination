# Skill Design

## Important Rule

Do not create one skill per Swagger endpoint.

Use generic global skills.

```text
Skill = reusable business capability
Swagger API = tenant-specific executable tool
MCP Tool = tenant-specific executable action
Database Query = tenant-specific data source
```

## Global Skills

| Skill Name | Description |
|---|---|
| swagger.endpoint.search | Search API endpoints from tenant Swagger docs |
| swagger.endpoint.explain | Explain endpoint purpose, parameters, and response |
| api.query.execute | Execute GET/list/search API endpoints |
| api.record.create | Execute POST/create API endpoints |
| api.record.update | Execute PATCH/PUT update API endpoints |
| api.record.delete | Execute DELETE API endpoints with approval |
| database.schema.search | Search database tables, columns, and relations |
| database.sql.generate | Generate safe SQL from business question |
| mcp.tool.discovery | Discover available MCP tools |
| mcp.tool.execute | Execute approved MCP tools |
| analytics.trend.analysis | Analyze business trends |
| response.card.generate | Format result as card view |

## Healthcare Skills

| Skill Name | Description |
|---|---|
| patient.list | Retrieve patient records with paging/filtering |
| appointment.upcoming.byPatient | Retrieve upcoming appointments for a specific patient |
| doctor.schedule.view | Show doctor schedule and availability |
| visit.history | Show patient visit history |
| billing.invoice.list | Retrieve invoices and billing status |
| lab.pending.tests | Show pending lab tests |
| pharmacy.inventory.lowstock | Show low-stock medicines |

## ERP Skills

| Skill Name | Description |
|---|---|
| customer.list | Retrieve customer records |
| customer.outstanding.balance | Show customer outstanding balance |
| sales.order.list | Retrieve sales orders |
| product.search | Search product by name/barcode/category |
| purchase.order.list | Retrieve purchase orders |
| inventory.stock.lookup | Retrieve stock status |
| finance.cashflow.report | Generate cash-flow report |
| hr.employee.list | Retrieve employee records |

## Skill Runtime Structure

In development, this may be represented as files:

```text
skill.json
prompt.md
response-schema.json
```

In production SaaS, store these as database records:

```text
GlobalSkillMaster
GlobalSkillIntentExample
GlobalSkillPrompt
GlobalSkillResponseSchema
TenantSkillEndpointMapping
TenantSkillMcpToolMapping
```

