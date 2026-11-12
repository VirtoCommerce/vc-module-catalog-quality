// Blade options; type aliases, not interfaces, so they fit openBlade's Record<string, unknown> options

/** CatalogQualityEntityTargets: the entity whose quality targets are picked from */
export type EntityTargetsOptions = {
  entityId: string;
  entityType: string;
  entityName?: string;
};

/** CatalogQualityDataDetails: the entity's quality data for a target */
export type DataDetailsOptions = EntityTargetsOptions & {
  targetId?: string;
  targetName?: string;
};
