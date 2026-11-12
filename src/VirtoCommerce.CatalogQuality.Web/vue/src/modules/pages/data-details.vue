<template>
  <VcBlade
    :title="$t('CATALOG_QUALITY.DATA.TITLE')"
    :subtitle="subtitle"
    :toolbar-items="toolbar"
    :loading="loading"
    width="50%"
  >
    <VcContainer>
      <!-- markdown-it runs with html disabled, so the agent's text cannot inject markup -->
      <!-- eslint-disable-next-line vue/no-v-html -->
      <div
        v-if="data?.suggestions"
        class="catalog-quality-report"
        v-html="reportHtml"
      />
      <div
        v-else-if="!loading"
        class="catalog-quality-report__empty"
      >
        {{ $t("CATALOG_QUALITY.DATA.NO_SUGGESTIONS") }}
      </div>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer } from "@vc-shell/framework/ui";
import MarkdownIt from "markdown-it";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import type { DataDetailsOptions } from "../types";
import type { QualityData } from "../../api_client/virtocommerce.catalogquality";

// legacy quality-data-details: the entity's quality suggestions for a target (read-only)
defineBlade({ url: "/catalog-quality-data", name: "CatalogQualityDataDetails", permissions: ["catalog-quality:read"] });

const { t } = useI18n({ useScope: "global" });
const { options } = useBlade<DataDetailsOptions>();

const data = ref<QualityData>();
const loading = ref(false);

const markdown = new MarkdownIt({ html: false, linkify: true });
const reportHtml = computed(() => markdown.render(data.value?.suggestions ?? ""));

const subtitle = computed(() =>
  [options.value?.entityName || options.value?.entityId, options.value?.targetName].filter(Boolean).join(" — "),
);

async function load() {
  if (!options.value?.entityId || !options.value.entityType) return;
  loading.value = true;
  try {
    data.value = await api.data.dataGetByEntity(options.value.entityId, options.value.entityType, options.value.targetId);
  } catch {
    // not scored yet
    data.value = undefined;
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  { id: "refresh", title: t("CATALOG_QUALITY.TOOLBAR.REFRESH"), icon: "lucide-refresh-cw", clickHandler: load },
]);

onMounted(load);
</script>

<style lang="scss" scoped>
// Styles for the rendered markdown report (headings, the criteria table, the suggestion list)
.catalog-quality-report {
  font-size: 0.875rem;
  line-height: 1.25rem;
  color: var(--color-neutral-800);

  &__empty {
    padding: 40px 0;
    text-align: center;
    font-size: 0.875rem;
    color: var(--color-neutral-500);
  }

  :deep(h1),
  :deep(h2),
  :deep(h3),
  :deep(h4) {
    font-weight: 700;
    margin: 16px 0 8px;
  }

  :deep(h1) {
    font-size: 1.25rem;
  }

  :deep(h2) {
    font-size: 1.125rem;
  }

  :deep(h3),
  :deep(h4) {
    font-size: 1rem;
  }

  :deep(p) {
    margin: 8px 0;
  }

  :deep(ul),
  :deep(ol) {
    margin: 8px 0;
    padding-left: 20px;
  }

  :deep(ul) {
    list-style: disc;
  }

  :deep(ol) {
    list-style: decimal;
  }

  :deep(li) {
    margin: 4px 0;
  }

  :deep(a) {
    color: var(--color-primary-500);
    text-decoration: underline;
  }

  :deep(code) {
    border-radius: 4px;
    padding: 0 4px;
    background-color: var(--color-neutral-100);
  }

  :deep(table) {
    width: 100%;
    margin: 12px 0;
    border-collapse: collapse;
  }

  :deep(th),
  :deep(td) {
    padding: 6px 8px;
    text-align: left;
    vertical-align: top;
    border: 1px solid var(--color-neutral-200);
  }

  :deep(th) {
    font-weight: 600;
    background-color: var(--color-neutral-50);
  }
}
</style>
