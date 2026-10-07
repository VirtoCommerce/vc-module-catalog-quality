# Catalog Quality

Catalog Quality helps catalog managers find products that are not ready to publish yet, and tells them what to fix.

An AI agent checks each product and gives it a **readiness score from 0 to 100**. It also writes a short list of **suggestions**, for example "add 2 more images" or "translate the full description to `fr-FR`". The score and suggestions are saved and shown in the Virto Commerce back office, next to the product.

## What you get

- **Product Quality Expert.** An AI agent you can ask in plain language:
  - *"Check data quality of product ABC-001002 in catalog My-Super-Catalog"*
  - *"What is the readiness score of product X?"*

  The agent finds the product, checks its data, saves the result and replies with the score, a breakdown and the suggestions. It never changes the product itself.
- **Product data score widget.** Shows the saved score on the product details blade. Click the widget to see the full suggestions.
- **Catalog quality workspace.** A main menu item (**Browse → Catalog quality**) that lists every evaluated product with its score and the time of its last evaluation.

## How a product is scored

The score is the sum of three parts:

| What is checked | Points | A product gets full points when… |
|---|---|---|
| Images | 30 | it has at least 3 images |
| Properties | 30 | every property has a value; multilanguage properties have a value in every catalog language |
| Descriptions | 40 | it has a description in the catalog's default language (10 points) and a proper translation of each description to every other catalog language (30 points) |

A translation only counts if it is real. The agent does not count these:

- a copy of the original text;
- a truncated text, or a one-line stub of a long text;
- a placeholder such as `TODO` or `lorem ipsum`.

If something is wrong, the suggestions say what and why.

Each product has **one** evaluation. When you evaluate a product again, the new result replaces the old one. The agent's reply also shows the previous score so you can see the progress, e.g. *"Previous score: 45 (2026-09-12) → now 70"*.

You can change the scoring rules and the style of the suggestions in the agent definition: [product-quality-expert.md](src/VirtoCommerce.CatalogQuality.Web/ai/agents/product-quality-expert.md).

## Requirements

- Virto Commerce Platform **3.1071.0** or later.
- **Catalog** module 3.1000.0 or later.
- **Assets** module 3.1000.0 or later. Suggestions are stored as files in the blob storage.
- An AI agent runtime that loads agents and tools from the module's `ai` folder and provides the catalog tools the agent uses (`vc_catalog_search_catalogs`, `vc_catalog_search_products`, `vc_catalog_get_products_with_descriptions`).

## Permissions

The module uses the Catalog module's permissions. It has none of its own.

| Action | Permission |
|---|---|
| See the menu item and the list | `catalog:access` |
| Read quality data | `catalog:read` |
| Save an evaluation | `catalog:update` |

## How the data is stored

- The score is saved in the `QualityData` table, together with the product id, entity type and audit fields. SQL Server, PostgreSQL and MySQL are supported. Database migrations run automatically on startup.
- The suggestions are saved as a Markdown file in the blob storage, at `catalog-quality/suggestions/<entity type>/<product id>.md`.

## Web API

All endpoints require authentication.

| Method | Endpoint | What it does |
|---|---|---|
| `POST` | `/api/catalogquality/search` | Search evaluations by `entityId`, `entityType`, with paging and sorting |
| `GET` | `/api/catalogquality/getbyentity?entityId=…&entityType=…` | Get the evaluation of one product |
| `POST` | `/api/catalogquality/update` | Create or overwrite an evaluation |

For catalog products, `entityType` is `VirtoCommerce.CatalogModule.Core.Model.CatalogProduct`.

## For developers

### Building

```bash
dotnet build VirtoCommerce.CatalogQuality.sln

cd src/VirtoCommerce.CatalogQuality.Web
npm install
npm run webpack:build   # or webpack:watch while developing
```

### Project layout

| Project | Contents |
|---|---|
| `VirtoCommerce.CatalogQuality.Core` | Domain model (`QualityData`), service interfaces, constants |
| `VirtoCommerce.CatalogQuality.Data` | EF Core repository, services, MediatR commands and queries |
| `VirtoCommerce.CatalogQuality.Data.SqlServer` / `.PostgreSql` / `.MySql` | Migrations for each database |
| `VirtoCommerce.CatalogQuality.Web` | Module entry point, API controller, AngularJS UI (`Scripts`), AI agent and tools (`ai`) |

### AI agent and tools

| File | Purpose |
|---|---|
| [ai/agents/product-quality-expert.md](src/VirtoCommerce.CatalogQuality.Web/ai/agents/product-quality-expert.md) | The agent: its workflow, scoring rules and suggestion format |
| [ai/tools/get-product-quality-data.yaml](src/VirtoCommerce.CatalogQuality.Web/ai/tools/get-product-quality-data.yaml) | Loads the saved evaluation of a product |
| [ai/tools/update-product-quality-data.yaml](src/VirtoCommerce.CatalogQuality.Web/ai/tools/update-product-quality-data.yaml) | Saves an evaluation |

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
