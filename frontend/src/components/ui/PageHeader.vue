<template>
  <header class="admin-page-header">
    <!-- Breadcrumbs -->
    <nav v-if="breadcrumbs && breadcrumbs.length" class="basis-full flex items-center gap-1.5 text-xs" style="color: var(--admin-text-subtle);">
      <template v-for="(item, index) in breadcrumbs" :key="index">
        <router-link
          v-if="item.to"
          :to="item.to"
          class="hover:opacity-80 transition-opacity"
          style="color: var(--admin-accent);"
        >
          {{ item.label }}
        </router-link>
        <span v-else>{{ item.label }}</span>
        <span v-if="index < breadcrumbs.length - 1" class="opacity-60">/</span>
      </template>
    </nav>

    <!-- Header Content -->
    <div class="flex-1 min-w-0">
      <h1 class="admin-page-title">{{ title }}</h1>
      <p v-if="subtitle" class="admin-page-subtitle">{{ subtitle }}</p>
    </div>
    <div v-if="$slots.default || $slots.actions" class="admin-page-actions">
      <div v-if="$slots.actions" class="admin-page-actions-frame">
        <slot name="actions" />
      </div>
      <slot />
    </div>
  </header>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true
  },
  subtitle: {
    type: String,
    default: ''
  },
  breadcrumbs: {
    type: Array,
    default: () => []
  }
})
</script>
