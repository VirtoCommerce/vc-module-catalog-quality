<template>
  <VcBlade
    :title="param ? (original.name ?? '') : $t('CATALOG_QUALITY.TARGET.TITLE_NEW')"
    :subtitle="param ? $t('CATALOG_QUALITY.TARGET.SUBTITLE') : undefined"
    :toolbar-items="toolbar"
    :loading="loading"
    width="70%"
  >
    <VcContainer>
      <VcForm class="catalog-quality-form">
        <VcSelect
          v-model="item.entityType"
          :label="$t('CATALOG_QUALITY.TARGET.ENTITY_TYPE')"
          :placeholder="$t('CATALOG_QUALITY.TARGET.ENTITY_TYPE_PLACEHOLDER')"
          :options="entityTypeOptions"
          :clearable="false"
          :disabled="readonly"
          name="entityType"
          rules="required"
          required
        />
        <VcInput
          v-model="item.name"
          :label="$t('CATALOG_QUALITY.TARGET.NAME')"
          :disabled="readonly"
          maxlength="256"
          name="name"
          rules="required"
          required
        />
        <VcTextarea
          v-model="item.description"
          :label="$t('CATALOG_QUALITY.TARGET.DESCRIPTION')"
          :placeholder="$t('CATALOG_QUALITY.TARGET.DESCRIPTION_PLACEHOLDER')"
          :disabled="readonly"
          maxlength="1024"
          name="description"
        />
        <VcTextarea
          v-model="item.prompt"
          class="catalog-quality-form__prompt"
          :label="$t('CATALOG_QUALITY.TARGET.PROMPT')"
          :placeholder="$t('CATALOG_QUALITY.TARGET.PROMPT_PLACEHOLDER')"
          :disabled="readonly"
          :maxlength="PROMPT_MAX_LENGTH"
          name="prompt"
        />
      </VcForm>
    </VcContainer>
  </VcBlade>
</template>

<script setup lang="ts">
import { useBlade, useBladeForm, usePermissions, usePopup, type IBladeToolbar } from "@vc-shell/framework";
import { VcBlade, VcContainer, VcForm, VcInput, VcSelect, VcTextarea } from "@vc-shell/framework/ui";
import { computed, onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { api } from "../api";
import { entityTypeName, loadEntityTypes } from "../entityTypes";
import type { QualityEntityType, QualityTarget } from "../../api_client/virtocommerce.catalogquality";

// legacy quality-target-details
defineBlade({ url: "/catalog-quality-target", name: "CatalogQualityTargetDetails", permissions: ["catalog-quality:read"] });

// The prompt is unlimited in the database; VcTextarea limits to 1024 characters by default
const PROMPT_MAX_LENGTH = "1000000";

const { t } = useI18n({ useScope: "global" });
const { param, callParent, closeSelf } = useBlade();
const { showConfirmation, showError } = usePopup();
const { hasAccess } = usePermissions();

const item = ref<QualityTarget>({});
// the saved target: the blade title shows its name, not the one being typed
const original = ref<QualityTarget>({});
const loading = ref(false);
const readonly = computed(() => !hasAccess("catalog-quality:update"));
const form = useBladeForm({ data: item, closeConfirmMessage: computed(() => t("CATALOG_QUALITY.TARGET.CLOSE_CONFIRM")) });

const entityTypes = ref<QualityEntityType[]>([]);
loadEntityTypes().then((x) => (entityTypes.value = x));
// a target of a type no installed module registers anymore keeps its type as an option, so it can still be saved
const entityTypeOptions = computed(() => {
  const options = entityTypes.value.map((x) => ({ id: x.id!, title: x.name ?? x.id! }));
  const current = item.value.entityType;
  if (current && !options.some((x) => x.id.toLowerCase() === current.toLowerCase())) {
    options.push({ id: current, title: entityTypeName(entityTypes.value, current)! });
  }
  return options;
});

async function load() {
  loading.value = true;
  try {
    item.value = param.value ? await api.targets.targetGetById(param.value) : {};
    original.value = { ...item.value };
  } finally {
    loading.value = false;
  }
  form.setBaseline();
}

async function save() {
  loading.value = true;
  try {
    await api.targets.targetUpdate(item.value);
    form.setBaseline();
    callParent("reload");
    // The update endpoint returns no id, so a new target is shown from the refreshed list
    if (!param.value) {
      closeSelf();
      return;
    }
    await load();
  } catch (e) {
    showError(String((e as Error).message ?? e));
  } finally {
    loading.value = false;
  }
}

const toolbar = computed<IBladeToolbar[]>(() => [
  {
    id: "save",
    title: t("CATALOG_QUALITY.TOOLBAR.SAVE"),
    icon: "lucide-save",
    permissions: "catalog-quality:update",
    disabled: !form.canSave.value || !item.value.name || !item.value.entityType,
    clickHandler: save,
  },
  {
    id: "reset",
    title: t("CATALOG_QUALITY.TOOLBAR.RESET"),
    icon: "lucide-undo-2",
    permissions: "catalog-quality:update",
    disabled: !form.isModified.value,
    clickHandler: load,
  },
  {
    id: "delete",
    title: t("CATALOG_QUALITY.TOOLBAR.DELETE"),
    icon: "lucide-trash-2",
    // a new target is not saved yet, so there is nothing to delete
    isVisible: !!param.value,
    permissions: "catalog-quality:delete",
    disabled: loading.value,
    async clickHandler() {
      if (!(await showConfirmation(t("CATALOG_QUALITY.TARGET.DELETE_CONFIRM", { name: original.value.name ?? "" })))) return;
      loading.value = true;
      try {
        await api.targets.targetDelete([param.value!]);
        // deleted: drop unsaved changes, otherwise closing the blade asks to save them
        form.setBaseline();
        callParent("reload");
        closeSelf();
      } catch (e) {
        showError(String((e as Error).message ?? e));
      } finally {
        loading.value = false;
      }
    },
  },
]);

onMounted(load);
</script>

<style lang="scss" scoped>
.catalog-quality-form {
  display: flex;
  flex-direction: column;
  gap: 16px;

  // the legacy prompt editor was 25 rows high
  &__prompt :deep(textarea) {
    min-height: 480px;
  }
}
</style>
