import {
  CatalogQualityDataClient,
  CatalogQualityEntityTypeClient,
  CatalogQualityTargetClient,
} from "../api_client/virtocommerce.catalogquality";

// The generated clients are plain fetch wrappers (same-origin cookie auth), so one instance each is enough.
export const api = {
  data: new CatalogQualityDataClient(),
  targets: new CatalogQualityTargetClient(),
  entityTypes: new CatalogQualityEntityTypeClient(),
};
