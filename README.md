# Catalog Quality

Catalog Quality helps catalog managers find products that are not ready to publish yet, and tells them what to fix.

You describe what "ready" means in **quality targets**, for example "Publication readiness for Chinese marketplaces" or "Amazon Germany". An AI agent checks a product against a target and gives it a **readiness score from 0 to 100**. It also writes a short list of **suggestions**, for example "add 2 more images" or "translate the full description to `fr-FR`". The score and suggestions are saved and shown in the Virto Commerce back office, next to the product.

## What you get

- **Quality targets.** A main menu item (**Browse → Quality targets**) where you create, edit and delete targets. Each target has:
  - **Entity type** — what the target evaluates. Today this is *Catalog product*.
  - **Name** and **Description** — the agent uses them to pick the right target from the user's request.
  - **Prompt** — the scoring rules: criteria, their weights, required languages, and so on.
- **Product Quality Expert.** An AI agent you can ask in plain language:
  - *"Check data quality of product ABC-001002 in catalog My-Super-Catalog"*
  - *"Check publication readiness of product ABC-001002 for China"*
  - *"Score product X against all targets"*

  The agent finds the product and the matching target(s), checks the product data against each target's prompt, saves the results and replies with the score, a breakdown and the suggestions. It never changes the product itself.
- **Product data score widget.** On the product details blade. Click it to see the targets, then click a target to see the product's saved score and suggestions for it.

## How a product is scored

The target's **prompt is the only source of scoring rules**. The agent evaluates exactly the criteria the prompt defines and nothing else.

- If the prompt gives weights (maximum points) per criterion, the agent uses them, scaled to 100 if they don't add up.
- If the prompt gives no weights, 100 points are split equally between its criteria.
- Inside a criterion, points are given in proportion to how much of it is met, e.g. 2 of 3 required images → 2/3 of the points.
- When a criterion involves languages, the agent takes them from the product's catalog (the default language is the source, the others are translations), unless the prompt names the languages itself.

An example target prompt:

> Evaluate two criteria:
> 1 - product name localization (30 points max).
> 2 - product descriptions translations (70 points max).

The suggestions start with the score and a table with the points per criterion, followed by one actionable bullet per criterion that lost points.

A product has **one evaluation per target**. When you evaluate it against the same target again, the new result replaces the old one. The agent's reply also shows the previous score so you can see the progress, e.g. *"Previous score: 45 (2026-09-12) → now 70"*.

You can change the agent's workflow and the style of the suggestions in the agent definition: [product-quality-expert.md](src/VirtoCommerce.CatalogQuality.Web/ai/agents/product-quality-expert.md).

## Requirements

- Virto Commerce Platform **3.1071.0** or later.
- **Catalog** module 3.1000.0 or later.
- **Assets** module 3.1000.0 or later. Suggestions are stored as files in the blob storage.
- An AI agent runtime that loads agents and tools from the module's `ai` folder and provides the catalog tools the agent uses (`vc_catalog_search_catalogs`, `vc_catalog_search_products`, `vc_catalog_get_products_with_descriptions`).

## Permissions

| Action | Permission |
|---|---|
| See the **Quality targets** menu item | `catalog-quality:access` |
| Read targets and evaluations, see the product widget | `catalog-quality:read` |
| Create and edit targets, save evaluations | `catalog-quality:update` |
| Delete targets | `catalog-quality:delete` |

## How the data is stored

- Targets are saved in the `QualityTarget` table.
- The score of each evaluation is saved in the `QualityData` table, together with the entity id, entity type, target id and audit fields.
- The suggestions are saved as a Markdown file in the blob storage, at `catalog-quality/suggestions/<entity type>/<entity id>/<target id>.md`.
- SQL Server, PostgreSQL and MySQL are supported. Database migrations run automatically on startup.

## Web API

All endpoints require authentication and the permission shown.

### Evaluations — `/api/catalogquality/data`

| Method | Endpoint | Permission | What it does |
|---|---|---|---|
| `POST` | `/search` | `read` | Search evaluations by `entityId`, `entityType`, with paging and sorting |
| `GET` | `/getbyentity?entityId=…&entityType=…&targetId=…` | `read` | Get the evaluation of one entity for one target |
| `POST` | `/update` | `update` | Create or overwrite one evaluation |
| `POST` | `/bulkupdate` | `update` | Create or overwrite several evaluations in one call; matched by entity and target, no record ids needed |

### Targets — `/api/catalogquality/target`

| Method | Endpoint | Permission | What it does |
|---|---|---|---|
| `POST` | `/search` | `read` | Search targets by `keyword` (name), `entityType` and `objectIds`, with paging and sorting |
| `GET` | `/{id}` | `read` | Get one target with its prompt |
| `POST` | `/update` | `update` | Create or update a target |
| `DELETE` | `/delete?ids=…` | `delete` | Delete targets |

For catalog products, `entityType` is `VirtoCommerce.CatalogModule.Core.Model.CatalogProduct`.

## For developers

### Building

```bash
dotnet build VirtoCommerce.CatalogQuality.sln

cd src/VirtoCommerce.CatalogQuality.Web
npm install
npm run webpack:build   # or webpack:watch while developing
```

### Testing

```bash
dotnet test tests/VirtoCommerce.CatalogQuality.Tests
```

### Project layout

| Project | Contents |
|---|---|
| `VirtoCommerce.CatalogQuality.Core` | Domain model (`QualityData`, `QualityTarget`), events, service interfaces, constants |
| `VirtoCommerce.CatalogQuality.Data` | EF Core repository, services, MediatR commands and queries |
| `VirtoCommerce.CatalogQuality.Data.SqlServer` / `.PostgreSql` / `.MySql` | Migrations for each database |
| `VirtoCommerce.CatalogQuality.Web` | Module entry point, API controllers, AngularJS UI (`Scripts`), AI agent and tools (`ai`) |
| `tests/VirtoCommerce.CatalogQuality.Tests` | Unit tests |

### Events

`TargetChangedEvent` is published when an existing target is updated. It carries the old and the new target values.

### Adding an entity type

Targets and evaluations are not tied to catalog products. To evaluate another kind of entity, register its type in the admin UI so it appears in the target's **Entity type** list:

```js
qualityDataTypes.addType({
    caption: 'quality-data-entity-types.my-entity',   // localization key
    value: 'My.Module.Core.Model.MyEntity'            // full type name, stored as EntityType
});
```

Then give an agent the generic quality tools below with that `entityType`.

### AI agent and tools

| File | Purpose |
|---|---|
| [ai/agents/product-quality-expert.md](src/VirtoCommerce.CatalogQuality.Web/ai/agents/product-quality-expert.md) | The catalog product agent: its workflow, scoring rules and suggestion format |
| [ai/tools/search-quality-targets.yaml](src/VirtoCommerce.CatalogQuality.Web/ai/tools/search-quality-targets.yaml) | Lists the targets of one entity type (id, name, description) |
| [ai/tools/get-quality-target.yaml](src/VirtoCommerce.CatalogQuality.Web/ai/tools/get-quality-target.yaml) | Loads one target by id, with its prompt |
| [ai/tools/get-quality-data.yaml](src/VirtoCommerce.CatalogQuality.Web/ai/tools/get-quality-data.yaml) | Loads the saved evaluation of an entity for a target |
| [ai/tools/update-quality-data.yaml](src/VirtoCommerce.CatalogQuality.Web/ai/tools/update-quality-data.yaml) | Saves several evaluations in one call |

## License

Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

<https://virtocommerce.com/open-source-license>

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
