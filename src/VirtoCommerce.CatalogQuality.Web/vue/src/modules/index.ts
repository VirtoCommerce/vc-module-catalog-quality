import * as blades from "./pages";
import * as locales from "./locales";
import { defineAppModule, registerExternalWidget } from "@vc-shell/framework";
import { defineComponent, h, markRaw } from "vue";
import QualityDataWidget from "./components/QualityDataWidget.vue";
import { CATALOG_PRODUCT } from "./entityTypes";

// Product details: "Product data score" widget (legacy widget registered into "itemDetail" for CatalogProduct)
registerExternalWidget({
  id: "CatalogQualityProductDataScore",
  title: "Product data score",
  component: markRaw(
    defineComponent({
      name: "CatalogQualityProductDataScoreWidget",
      setup: () => () => h(QualityDataWidget, { entityType: CATALOG_PRODUCT }),
    }),
  ),
  targetBlades: ["CatalogProductDetails"],
  // not on the unsaved "New product" blade
  isVisible: (blade) => !!blade?.param,
});

export default defineAppModule({
  blades,
  locales,
});

export * from "./pages";
