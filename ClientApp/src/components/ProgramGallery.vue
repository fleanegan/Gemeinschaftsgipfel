<template>
  <div ref="galleryRef" class="program-gallery">
    <div v-if="isVisible" class="program-images">
      <img
        v-for="program in programs"
        :key="program.year"
        :src="program.url"
        :alt="`Programm ${program.year}`"
        class="program-image"
        loading="lazy"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue';

interface ProgramImage {
  year: number;
  url: string;
}

// Discover program SVGs at build-time using Vite's import.meta.glob
const programModules = import.meta.glob('/public/programs/*.svg', { eager: false, as: 'url' });

// Extract years from filenames and sort descending (2025, 2024, 2023...)
const programs = ref<ProgramImage[]>([]);

// Parse filenames and create sorted program list
Object.keys(programModules).forEach((path) => {
  // Extract filename: /public/programs/2024.svg -> 2024.svg
  const filename = path.split('/').pop();
  if (filename) {
    // Extract year: 2024.svg -> 2024
    const yearMatch = filename.match(/^(\d{4})\.svg$/);
    if (yearMatch && yearMatch[1]) {
      const year = parseInt(yearMatch[1], 10);
      // Convert path to public URL: /public/programs/2024.svg -> /programs/2024.svg
      const url = path.replace('/public', '');
      programs.value.push({ year, url });
    }
  }
});

// Sort descending (newest first)
programs.value.sort((a, b) => b.year - a.year);

const galleryRef = ref<HTMLElement | null>(null);
const isVisible = ref(false);
let observer: IntersectionObserver | null = null;

onMounted(() => {
  // Set up IntersectionObserver for lazy loading
  if (!galleryRef.value) return;

  observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          isVisible.value = true;
          // Stop observing once visible
          if (observer && galleryRef.value) {
            observer.unobserve(galleryRef.value);
          }
        }
      });
    },
    {
      rootMargin: '50px', // Start loading 50px before element enters viewport
    }
  );

  observer.observe(galleryRef.value);
});

onUnmounted(() => {
  if (observer && galleryRef.value) {
    observer.unobserve(galleryRef.value);
  }
});
</script>

<style scoped>
.program-gallery {
  width: 100%;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 2rem 0;
  min-height: 200px; /* Reserve space to prevent layout shift */
}

.program-images {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 2rem;
  width: 100%;
  max-width: 800px;
  padding: 0 1rem;
}

.program-image {
  width: 100%;
  max-width: 800px;
  height: auto;
  display: block;
}
</style>
