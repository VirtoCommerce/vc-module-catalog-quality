import { api } from "./api";
import type { QualityEntityType } from "../api_client/virtocommerce.catalogquality";

export const CATALOG_PRODUCT = "VirtoCommerce.CatalogModule.Core.Model.CatalogProduct";

let entityTypes: Promise<QualityEntityType[]> | undefined;

/**
 * Entity types quality targets can be defined for, as the installed modules registered them on the server
 * (IQualityEntityTypeRegistrar). Loaded once; a failed load is retried on the next call.
 */
export function loadEntityTypes(): Promise<QualityEntityType[]> {
  entityTypes ??= api.entityTypes
    .getEntityTypes()
    .then((x) => x ?? [])
    .catch(() => {
      entityTypes = undefined;
      return [];
    });
  return entityTypes;
}

/** Display name of an entity type; an unregistered one shows its id. */
export const entityTypeName = (types: QualityEntityType[], id?: string) =>
  types.find((x) => x.id?.toLowerCase() === id?.toLowerCase())?.name ?? id;
