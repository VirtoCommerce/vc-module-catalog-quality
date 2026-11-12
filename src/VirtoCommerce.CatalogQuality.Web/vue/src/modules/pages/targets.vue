<template>
  <VcBlade
    :title="$t('CATALOG_QUALITY.TARGETS.TITLE')"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcDataTable
      v-model:selection="selection"
      v-model:sort-field="sortField"
      v-model:sort-order="sortOrder"
      selection-mode="multiple"
      :items="items"
      :total-count="totalCount"
      :pagination="pagination"
      :state-key="'CATALOG_QUALITY_TARGETS'"
      :active-item-id="activeItemId"
      :empty-state="{ icon: 'lucide-target', title: $t('CATALOG_QUALITY.TARGETS.EMPTY') }"
      @row-click="(e: { data: QualityTarget }) => edit(e.data.id)"
      @pagination-click="pagination.goToPage"
    >
      <VcColumn
        id="name"
        :title="$t('CATALOG_QUALITY.TARGETS.NAME')"
        sortable
        always-visible
      />
      <VcColumn
        id="entityType"
        :title="$t('CATALOG_QUALITY.TARGETS.ENTITY_TYPE')"
      >
        <template #body="{ data }">{{ entityTypeName(entityTypes, data.entityType) }}</template>
      </VcColumn>
    </VcDataTable>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useDataTablePagination, useDataTableSort, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcColumn, VcDataTable } from "@vc-shell/framework/ui";
import { computed, ref, watch } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { entityTypeName, loadEntityTypes } from "../entityTypes";
import type { QualityEntityType, QualityTarget } from "../../api_client/virtocommerce.catalogquality";

// Quality targets workspace (legacy quality-target-list opened from the main menu)
defineBlade({
  url: "/catalog-quality",
  name: "CatalogQualityTargets",
  isWorkspace: true,
  permissions: ["catalog-quality:access"],
  menuItem: { title: "CATALOG_QUALITY.MENU.TITLE", icon: "lucide-target", priority: 400 },
});

const { t } = useI18n({ useScope: "global" });
const { openBlade, closeChildren, exposeToChildren } = useBlade();
const { showConfirmation } = usePopup();

const items = ref<QualityTarget[]>([]);
const totalCount = ref(0);
const loading = ref(false);
const selection = ref<QualityTarget[]>([]);
const activeItemId = ref<string>();
const { sortField, sortOrder, sortExpression } = useDataTableSort({ initialField: "name", initialDirection: "ASC" });
const PAGE_SIZE = 20;
const pagination = useDataTablePagination({ pageSize: PAGE_SIZE, totalCount });

const entityTypes = ref<QualityEntityType[]>([]);
loadEntityTypes().then((x) => (entityTypes.value = x));

async function load() {
  loading.value = true;
  try {
    const res = await api.targets.targetSearch({ sort: sortExpression.value, skip: pagination.skip, take: PAGE_SIZE });
    items.value = res.results ?? [];
    totalCount.value = res.totalCount ?? 0;
  } finally {
    loading.value = false;
  }
}
load();
watch([sortExpression, () => pagination.skip], load);

function edit(id?: string) {
  activeItemId.value = id;
  openBlade({ name: "CatalogQualityTargetDetails", param: id, onClose: () => (activeItemId.value = undefined) });
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG_QUALITY.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
  {
    id: "add",
    title: t("CATALOG_QUALITY.TOOLBAR.ADD"),
    icon: "lucide-plus",
    permissions: "catalog-quality:update",
    clickHandler: () => edit(),
  },
  {
    id: "delete",
    title: t("CATALOG_QUALITY.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    permissions: "catalog-quality:delete",
    disabled: !selection.value.length,
    async clickHandler() {
      if (!selection.value.length) return;
      if (!(await showConfirmation(t("CATALOG_QUALITY.TARGETS.DELETE_CONFIRM")))) return;
      await closeChildren();
      loading.value = true;
      try {
        await api.targets.targetDelete(selection.value.map((x) => x.id!));
        selection.value = [];
      } finally {
        loading.value = false;
      }
      await load();
    },
  },
]);

exposeToChildren({ reload: load });
</script>
