<template>
  <VcBlade
    :title="$t('CATALOG_QUALITY.TARGETS.TITLE')"
    :subtitle="options?.entityName"
    :toolbar-items="toolbar"
    :loading="loading"
    width="40%"
  >
    <VcDataTable
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :state-key="'CATALOG_QUALITY_ENTITY_TARGETS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-target', title: $t('CATALOG_QUALITY.TARGETS.EMPTY') }"
      @row-click="(e: { data: QualityTarget }) => open(e.data)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG_QUALITY.TARGETS.NAME')"
        sortable
        always-visible
      />
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, useDataTableSort, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { DataDetailsOptions, EntityTargetsOptions } from "../types";
import type { QualityTarget } from "../../api_client/virtocommerce.catalogquality";

// Legacy quality-target-list in entity context (opened from an entity's quality widget, e.g. on a product): only the
// targets of the entity's type, for picking; a picked target opens the entity's quality data for it.
// Targets are managed from the main menu.
defineBlade({ url: "/catalog-quality-entity-targets", name: "CatalogQualityEntityTargets", permissions: ["catalog-quality:read"] });

const { t } = useI18n({ useScope: "global" });
const { options, openBlade } = useBlade<EntityTargetsOptions>();

const items = ref<QualityTarget[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const activeItemId = ref<string>();
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "name", initialDirection: "ASC" });
const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

async function load() {
  loading.value = true;
  try {
    const res = await api.targets.targetSearch({
      entityType: options.value?.entityType,
      sort: sortExpression.value,
      skip: pagination.skip,
      take: PAGE_SIZE,
    });
    items.value = res.results ?? [];
    totalCount.value = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}
load();
watch([sortExpression, () => pagination.skip], load);

function open(target: QualityTarget) {
  if (!options.value) return;
  activeItemId.value = target.id;
  const dataOptions: DataDetailsOptions = { ...options.value, targetId: target.id, targetName: target.name };
  openBlade({
    name: "CatalogQualityDataDetails",
    options: dataOptions,
    onClose: () => (activeItemId.value = undefined),
  });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG_QUALITY.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
]);
</script>
