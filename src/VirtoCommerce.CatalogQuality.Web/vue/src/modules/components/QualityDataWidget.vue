<template>
  <!-- legacy quality-data-widget on the product blade ("itemDetail"): opens the targets of the entity's type;
       picking one opens the entity's quality data for it -->
  <VcWidget
    v-if="hasAccess('catalog-quality:read')"
    icon="lucide-target"
    :title="$t('CATALOG_QUALITY.WIDGET.TITLE')"
    :disabled="!entity?.id"
    @click="open"
  />
</template>

<script setup lang="ts">
import { injectBladeContext, useBlade, usePermissions } from "@vc-shell/framework";
import { VcWidget } from "@vc-shell/framework/ui";
import { computed } from "vue";
import type { EntityTargetsOptions } from "../types";

const props = defineProps<{ entityType: string }>();

// the entity shared by the host blade via defineBladeContext({ item })
const ctx = injectBladeContext();
const entity = computed(() => ctx.value.item as { id?: string; name?: string } | undefined);
const { openBlade } = useBlade();
const { hasAccess } = usePermissions();

function open() {
  if (!entity.value?.id) return;
  const options: EntityTargetsOptions = { entityId: entity.value.id, entityType: props.entityType, entityName: entity.value.name };
  openBlade({ name: "CatalogQualityEntityTargets", options });
}
</script>
