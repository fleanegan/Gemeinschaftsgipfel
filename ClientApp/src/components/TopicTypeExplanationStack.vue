<template>
  <div 
    ref="containerRef"
    class="stack-container"
    :class="{ 'is-mobile': isMobile }"
    @mouseenter="handleMouseEnter"
    @mouseleave="handleMouseLeave"
    @click="handleClick"
  >
    <!-- Mobile: scrollable wrapper -->
    <div 
      v-if="isMobile" 
      ref="scrollContainerRef"
      class="scroll-container"
      @scroll="handleScroll"
    >
      <div
        v-for="(format, index) in formats"
        :key="format.type"
        class="format-card"
        :class="[`format-${format.type.toLowerCase()}`, { 'no-background': !isImageLoaded(format.photo) }]"
        :style="{ opacity: cardOpacities[index] }"
      >
        <div v-if="isImageLoaded(format.photo)" class="card-background">
          <img :src="format.photo" :alt="format.type" />
        </div>
        <div v-if="isImageLoaded(format.photo)" class="card-overlay"></div>
        <div class="card-content">
          <span v-if="format.type !== 'Explanation'" class="card-badge" :class="`category-${format.type.toLowerCase()}`">{{ format.type }}</span>
          <h3 v-if="format.type === 'Explanation'" class="card-title" :class="`title-${format.type.toLowerCase()}`">{{ format.title }}</h3>
          <p class="card-description">{{ format.description }}</p>
        </div>
      </div>
    </div>

    <!-- Desktop: stacked layout (collapsed state) -->
    <div v-else class="stack-wrapper">
      <!-- Grey placeholder showing where cards came from -->
      <div v-if="isExpanded" class="stack-placeholder"></div>
      
      <!-- Actual stacked cards (hidden when expanded) -->
      <div
        v-for="(format, index) in formats"
        v-show="!isExpanded"
        :key="format.type"
        class="format-card is-stacked"
        :class="[`format-${format.type.toLowerCase()}`, { 'no-background': !isImageLoaded(format.photo), 'is-wiggling': isHovering }]"
        :style="getStackedCardStyle(index)"
      >
        <div v-if="isImageLoaded(format.photo)" class="card-background">
          <img :src="format.photo" :alt="format.type" />
        </div>
        <div v-if="isImageLoaded(format.photo)" class="card-overlay"></div>
        <div class="card-content">
          <span v-if="format.type !== 'Explanation'" class="card-badge" :class="`category-${format.type.toLowerCase()}`">{{ format.type }}</span>
          <h3 v-if="format.type === 'Explanation'" class="card-title" :class="`title-${format.type.toLowerCase()}`">{{ format.title }}</h3>
          <p class="card-description">{{ format.description }}</p>
        </div>
      </div>
    </div>
    
    <!-- Hint text when collapsed -->
    <div v-if="!isMobile && !isExpanded" class="stack-hint">
      <span>Formate erkunden</span>
    </div>
  </div>
  
  <!-- Fullscreen overlay for expanded cards (teleported to body) -->
  <Teleport to="body">
    <div 
      v-if="isExpanded && !isMobile" 
      class="expanded-overlay"
      :class="{ 'is-animating-in': isAnimatingIn, 'is-animating-out': isAnimatingOut }"
      @click="handleOverlayClick"
      @mouseleave="handleOverlayMouseLeave"
    >
      <div class="expanded-grid" :style="expandedGridStyle">
        <div
          v-for="(format, index) in formats"
          :key="format.type"
          class="format-card expanded-card"
          :class="[`format-${format.type.toLowerCase()}`, { 'no-background': !isImageLoaded(format.photo) }]"
          :style="getExpandedCardStyle(index)"
          @click.stop
        >
          <div v-if="isImageLoaded(format.photo)" class="card-background">
            <img :src="format.photo" :alt="format.type" />
          </div>
          <div v-if="isImageLoaded(format.photo)" class="card-overlay"></div>
          <div class="card-content">
            <span v-if="format.type !== 'Explanation'" class="card-badge" :class="`category-${format.type.toLowerCase()}`">{{ format.type }}</span>
            <h3 v-if="format.type === 'Explanation'" class="card-title" :class="`title-${format.type.toLowerCase()}`">{{ format.title }}</h3>
            <p class="card-description">{{ format.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';

type TopicType = 'Explanation' | 'Workshop' | 'Vortrag' | 'Sport' | 'Diskussion' | 'Sonstiges';

interface FormatInfo {
  type: TopicType;
  title: string;
  description: string;
  photo: string;
  // Random offsets for messy stack effect (seeded per card)
  offsetX: number;
  offsetY: number;
  rotation: number;
}

// Seeded random for consistent "messiness" across renders
function seededRandom(seed: number): number {
  const x = Math.sin(seed * 9999) * 10000;
  return x - Math.floor(x);
}

// Photo mapping for each topic type
const photoMap: Record<TopicType, string> = {
  'Explanation': '', // No photo for explanation card
  'Workshop': '/photos/workshop.jpg',
  'Vortrag': '/photos/presentation.jpg',
  'Sport': '/photos/sport.jpg',
  'Diskussion': '/photos/discussion.jpg',
  'Sonstiges': '/photos/misc.jpg',
};

const baseFormats: { type: TopicType; title: string; description: string }[] = [
  { type: 'Explanation', title: 'Thementypen', description: 'Jeder Vorschlag kann von einem anderen Typ sein. Hier sind die verfügbaren Typen.' },
  { type: 'Workshop', title: 'Mitmach-Session', description: 'Interaktive Sessions zum gemeinsamen Lernen und Ausprobieren.' },
  { type: 'Vortrag', title: 'Präsentation', description: 'Klassische Vorträge zu spannenden Themen.' },
  { type: 'Sport', title: 'Bewegung', description: 'Gemeinsame sportliche Aktivitäten und Spiele.' },
  { type: 'Diskussion', title: 'Austausch', description: 'Offene Gesprächsrunden zu kontroversen oder interessanten Themen.' },
  { type: 'Sonstiges', title: 'Kreativ', description: 'Alles, was in keine andere Kategorie passt.' },
];

// Card dimensions - Central definitions
const STACK_CARD_HEIGHT = 280;
const STACK_CARD_ASPECT_RATIO = 600 / 320; // Keep original aspect ratio (based on original 320px height)
const STACK_CARD_WIDTH = STACK_CARD_HEIGHT * STACK_CARD_ASPECT_RATIO; 

// Expanded card dimensions - derived from stack dimensions
const EXPANSION_GROWTH_FACTOR = 1.25;
const EXPANDED_CARD_BASE_HEIGHT = STACK_CARD_HEIGHT * EXPANSION_GROWTH_FACTOR;
const EXPANDED_CARD_BASE_WIDTH = EXPANDED_CARD_BASE_HEIGHT * STACK_CARD_ASPECT_RATIO; 

// Layout constants - derived from card dimensions
const MOBILE_BREAKPOINT = 785;
const EXPANDED_GRID_GAP = Math.round(STACK_CARD_HEIGHT * 0.137);
const EXPANDED_GRID_PADDING = EXPANDED_GRID_GAP * 2; // 48px (double the gap)

// Min/max scaling factors for expanded cards
const EXPANDED_CARD_MIN_FACTOR = 0.6; // Minimum size: 60% of base expanded size
const EXPANDED_CARD_MAX_FACTOR = 1.25; // Maximum size: 125% of base expanded size

// Calculate min/max dimensions maintaining aspect ratio
const EXPANDED_CARD_MIN_WIDTH = EXPANDED_CARD_BASE_WIDTH * EXPANDED_CARD_MIN_FACTOR; // ~240px
const EXPANDED_CARD_MIN_HEIGHT = EXPANDED_CARD_MIN_WIDTH / STACK_CARD_ASPECT_RATIO; // ~150px
const EXPANDED_CARD_MAX_WIDTH = EXPANDED_CARD_BASE_WIDTH * EXPANDED_CARD_MAX_FACTOR; // ~500px
const EXPANDED_CARD_MAX_HEIGHT = EXPANDED_CARD_MAX_WIDTH / STACK_CARD_ASPECT_RATIO; // ~312px

// Stack effect constants
const BASE_OFFSET_X = 2;
const BASE_OFFSET_Y = 2;
const RANDOM_OFFSET_X_RANGE = Math.round(BASE_OFFSET_X * 8); // 16px (8× base offset)
const RANDOM_OFFSET_Y_RANGE = Math.round(BASE_OFFSET_Y * 6); // 12px (6× base offset)
const RANDOM_ROTATION_RANGE = Math.round(BASE_OFFSET_X * 4); // 8 degrees (4× base offset)
const STACK_CARD_OPACITY_STEP = 0.12;
const STACK_CARD_MIN_OPACITY = 0.5;

// Animation constants
const ANIMATION_STAGGER_DELAY = 0.05; // seconds per card
const ANIMATION_IN_DURATION = 600; // ms
const ANIMATION_OUT_DURATION = 400; // ms
// Scale factor: stack card size relative to expanded card size (for FLIP animation)
const CARD_FLY_SCALE = STACK_CARD_WIDTH / EXPANDED_CARD_BASE_WIDTH;

// Mobile scroll constants
const MOBILE_FADE_OPACITY_MIN = 0.3;
const MOBILE_FADE_OPACITY_RANGE = 1.0 - MOBILE_FADE_OPACITY_MIN; // 0.7

// Offset buffers for random effects - derived from random ranges
const STACK_WIDTH_BUFFER = Math.round(RANDOM_OFFSET_X_RANGE * 1.25); // 20px (125% of x-range for safety margin)
const STACK_HEIGHT_BUFFER = Math.round(RANDOM_OFFSET_Y_RANGE * 1.67); // 20px (167% of y-range for safety margin)
const STACK_POSITION_OFFSET = BASE_OFFSET_X * 4; // 8px (4× base offset)

const formats: FormatInfo[] = baseFormats.map((format, index) => {
  // Get photo from photoMap
  const photo = photoMap[format.type];
  // Generate random offsets for each card (except the top one which stays neat)
  if (index === 0) {
    return { ...format, photo, offsetX: 0, offsetY: 0, rotation: 0 };
  }
  const seed = index * 42; // Consistent seed per card
  return {
    ...format,
    photo,
    offsetX: (seededRandom(seed) - 0.5) * RANDOM_OFFSET_X_RANGE,
    offsetY: (seededRandom(seed + 1) - 0.5) * RANDOM_OFFSET_Y_RANGE,
    rotation: (seededRandom(seed + 2) - 0.5) * RANDOM_ROTATION_RANGE,
  };
});

const containerRef = ref<HTMLElement | null>(null);
const scrollContainerRef = ref<HTMLElement | null>(null);

const isExpanded = ref(false);
const isAnimatingIn = ref(false);
const isAnimatingOut = ref(false);
const isMobile = ref(false);
const isHovering = ref(false);
const viewportWidth = ref(window.innerWidth);
const viewportHeight = ref(window.innerHeight);
const cardOpacities = ref<number[]>([1, 1, 1, 1, 1, 1]);
const stackPosition = ref({ x: 0, y: 0 });

// Track which images have successfully loaded
const loadedImages = ref<Set<string>>(new Set());

// Check if an image is loaded
function isImageLoaded(photo: string): boolean {
  return photo !== '' && loadedImages.value.has(photo);
}

// Preload images in the background
function preloadImages() {
  formats.forEach((format) => {
    if (format.photo) {
      const img = new Image();
      img.onload = () => {
        loadedImages.value = new Set([...loadedImages.value, format.photo]);
      };
      img.onerror = () => {
        // Image failed to load - don't add to loadedImages
        console.warn(`Failed to load image: ${format.photo}`);
      };
      img.src = format.photo;
    }
  });
}

// Detect portrait mode (taller than wide)
const isPortrait = computed(() => viewportHeight.value > viewportWidth.value);

// Calculate responsive expanded card dimensions based on viewport
const expandedCardSize = computed(() => {
  const vw = viewportWidth.value;
  const vh = viewportHeight.value;
  const paddingX = EXPANDED_GRID_PADDING;
  const paddingY = EXPANDED_GRID_PADDING;
  const gap = EXPANDED_GRID_GAP;
  
  // Portrait: 2 columns × 3 rows, Landscape: 3 columns × 2 rows
  const columns = isPortrait.value ? 2 : 3;
  const rows = isPortrait.value ? 3 : 2;
  
  // Calculate max card width based on column count
  const availableWidth = vw - paddingX;
  const maxWidthFromViewport = (availableWidth - gap * (columns - 1)) / columns;
  
  // Calculate max card height based on row count
  const availableHeight = vh - paddingY;
  const maxHeightFromViewport = (availableHeight - gap * (rows - 1)) / rows;
  
  // Maintain aspect ratio
  const aspectRatio = STACK_CARD_ASPECT_RATIO;
  
  // Start with width-based calculation
  let cardWidth = maxWidthFromViewport;
  let cardHeight = cardWidth / aspectRatio;
  
  // If height exceeds available space, recalculate based on height
  if (cardHeight > maxHeightFromViewport) {
    cardHeight = maxHeightFromViewport;
    cardWidth = cardHeight * aspectRatio;
  }
  
  // Apply reasonable min/max constraints
  cardWidth = Math.min(Math.max(cardWidth, EXPANDED_CARD_MIN_WIDTH), EXPANDED_CARD_MAX_WIDTH);
  cardHeight = Math.min(Math.max(cardHeight, EXPANDED_CARD_MIN_HEIGHT), EXPANDED_CARD_MAX_HEIGHT);
  
  return { width: cardWidth, height: cardHeight, gap, columns, rows };
});

// Computed style for the grid with responsive sizing
const expandedGridStyle = computed(() => {
  const { width, height, gap, columns, rows } = expandedCardSize.value;
  return {
    '--grid-card-width': `${width}px`,
    '--grid-card-height': `${height}px`,
    '--grid-gap': `${gap}px`,
    '--grid-columns': columns,
    '--grid-rows': rows,
  };
});

function getStackedCardStyle(index: number): Record<string, string | number> {
  const format = formats[index]!;
  // Combine base offset with random offset for messy pile effect
  const totalOffsetX = index * BASE_OFFSET_X + format.offsetX;
  const totalOffsetY = index * BASE_OFFSET_Y + format.offsetY;
  const rotation = format.rotation;
  
  return {
    position: 'absolute',
    '--card-offset-x': `${totalOffsetX}px`,
    '--card-offset-y': `${totalOffsetY}px`,
    '--card-rotation': `${rotation}deg`,
    transform: `translate(${totalOffsetX}px, ${totalOffsetY}px) rotate(${rotation}deg)`,
    zIndex: formats.length - index,
    opacity: index === 0 ? 1 : Math.max(STACK_CARD_MIN_OPACITY, 1 - index * STACK_CARD_OPACITY_STEP),
  };
}

function getExpandedCardStyle(index: number): Record<string, string | number> {
  const delay = index * ANIMATION_STAGGER_DELAY;
  const { width: cardWidth, height: cardHeight, gap, columns, rows } = expandedCardSize.value;
  
  // Calculate where this card will be in the grid (approximate center of viewport)
  const gridWidth = columns * cardWidth + (columns - 1) * gap;
  const gridHeight = rows * cardHeight + (rows - 1) * gap;
  
  // Grid starts at center of viewport
  const gridStartX = (viewportWidth.value - gridWidth) / 2;
  const gridStartY = (viewportHeight.value - gridHeight) / 2;
  
  // Calculate this card's position in the grid
  let cardGridX: number;
  let cardGridY: number;
  
  if (columns === 2) {
    // Portrait mode: 2 columns × 3 rows
    const row = Math.floor(index / 2);
    const col = index % 2;
    
    cardGridX = gridStartX + col * (cardWidth + gap);
    cardGridY = gridStartY + row * (cardHeight + gap);
  } else {
    // Landscape mode: 3 columns × 2 rows
    if (index < 3) {
      // First row
      cardGridX = gridStartX + index * (cardWidth + gap);
      cardGridY = gridStartY;
    } else {
      // Second row
      cardGridX = gridStartX + (index - 3) * (cardWidth + gap);
      cardGridY = gridStartY + cardHeight + gap;
    }
  }
  
  // Calculate offset from stack position to grid position
  const offsetX = stackPosition.value.x - cardGridX;
  const offsetY = stackPosition.value.y - cardGridY;
  
  return {
    '--offset-x': `${offsetX}px`,
    '--offset-y': `${offsetY}px`,
    '--card-delay': `${delay}s`,
    '--card-width': `${cardWidth}px`,
    '--card-height': `${cardHeight}px`,
  };
}

function openExpanded() {
  if (isExpanded.value || isAnimatingOut.value) return;
  
  // Capture stack position before expanding
  if (containerRef.value) {
    const rect = containerRef.value.getBoundingClientRect();
    stackPosition.value = { x: rect.left + STACK_POSITION_OFFSET, y: rect.top + STACK_POSITION_OFFSET };
  }
  isExpanded.value = true;
  isAnimatingIn.value = true;
  setTimeout(() => {
    isAnimatingIn.value = false;
  }, ANIMATION_IN_DURATION);
}

function closeExpanded() {
  if (!isExpanded.value || isAnimatingOut.value) return;
  
  isAnimatingOut.value = true;
  setTimeout(() => {
    isExpanded.value = false;
    isAnimatingOut.value = false;
  }, ANIMATION_OUT_DURATION);
}

function handleMouseEnter() {
  if (!isMobile.value && !isExpanded.value) {
    isHovering.value = true;
  }
}

function handleMouseLeave() {
  isHovering.value = false;
}

function handleClick() {
  if (!isMobile.value) {
    if (!isExpanded.value) {
      openExpanded();
    }
  }
}

function handleOverlayClick() {
  closeExpanded();
}

function handleOverlayMouseLeave() {
  closeExpanded();
}

function handleScroll() {
  if (!scrollContainerRef.value) return;
  
  const scrollLeft = scrollContainerRef.value.scrollLeft;
  
  // Calculate visibility ratio for first card (0 = scrolled out, 1 = fully visible)
  const firstCardVisibility = Math.max(0, 1 - scrollLeft / STACK_CARD_WIDTH);
  
  // Apply opacity: fade between min and full visibility
  cardOpacities.value = formats.map((_, index) => {
    if (index === 0) {
      return MOBILE_FADE_OPACITY_MIN + MOBILE_FADE_OPACITY_RANGE * firstCardVisibility;
    }
    return 1;
  });
}

function updateLayout() {
  isMobile.value = window.innerWidth < MOBILE_BREAKPOINT;
  viewportWidth.value = window.innerWidth;
  viewportHeight.value = window.innerHeight;
}

onMounted(() => {
  updateLayout();
  window.addEventListener('resize', updateLayout);
  preloadImages();
});

onUnmounted(() => {
  window.removeEventListener('resize', updateLayout);
});
</script>

<style scoped>
.stack-container {
  position: relative;
  width: 100%;
  cursor: pointer;
  margin-top: var(--space-sm); /* Align with instruction card content (accounting for enumerator offset) */
}

/* Desktop: Size for compact stacked cards with some extra room for random offsets */
.stack-container:not(.is-mobile) {
  max-width: 100%;
  width: calc(v-bind(STACK_CARD_WIDTH) * 1px + v-bind(STACK_WIDTH_BUFFER) * 1px);
  height: calc(v-bind(STACK_CARD_HEIGHT) * 1px + v-bind(STACK_HEIGHT_BUFFER) * 1px);
  min-width: 0;
  box-sizing: border-box;
}

/* Mobile scroll container */
.scroll-container {
  display: flex;
  gap: var(--space-sm);
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
  padding: var(--space-sm) 0;
  /* Break out of parent .wide-content padding to reach screen edges */
  margin-left: calc(-1 * var(--space-xl));
  margin-right: calc(-1 * var(--space-xl));
  padding-left: var(--space-xl);
  padding-right: var(--space-xl);
}

@media (max-width: 400px) {
  .scroll-container {
    margin-left: calc(-1 * var(--space-sm));
    margin-right: calc(-1 * var(--space-sm));
    padding-left: var(--space-sm);
    padding-right: var(--space-sm);
  }
}

.scroll-container::-webkit-scrollbar {
  display: none;
}

/* Desktop stack wrapper */
.stack-wrapper {
  position: relative;
  margin-left: calc(v-bind(STACK_POSITION_OFFSET) * 1px);
  margin-top: calc(v-bind(STACK_POSITION_OFFSET) * 1px);
  width: calc(v-bind(STACK_CARD_WIDTH) * 1px);
  height: calc(v-bind(STACK_CARD_HEIGHT) * 1px);
}

/* Grey placeholder showing where cards came from */
.stack-placeholder {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background-color: var(--color-border-light);
  border-radius: var(--radius-sharp);
  opacity: 0.5;
}

/* Fullscreen overlay */
.expanded-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  width: auto; /* use left/right so 100vw doesn't cause overflow */
  max-width: 100%;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 10000;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: var(--space-lg);
  box-sizing: border-box;
  opacity: 1;
  transition: opacity 0.3s ease;
}

.expanded-overlay.is-animating-in {
  animation: overlay-fade-in 0.3s ease forwards;
}

.expanded-overlay.is-animating-out {
  opacity: 0;
}

@keyframes overlay-fade-in {
  from { opacity: 0; }
  to { opacity: 1; }
}

/* CSS Grid for expanded cards */
.expanded-grid {
  --grid-card-width: calc(v-bind(EXPANDED_CARD_BASE_WIDTH) * 1px);
  --grid-card-height: calc(v-bind(EXPANDED_CARD_BASE_HEIGHT) * 1px);
  --grid-gap: calc(v-bind(EXPANDED_GRID_GAP) * 1px);
  --grid-columns: 3;
  --grid-rows: 2;
  
  display: grid;
  grid-template-columns: repeat(var(--grid-columns), var(--grid-card-width));
  grid-template-rows: repeat(var(--grid-rows), var(--grid-card-height));
  gap: var(--grid-gap);
  justify-content: center;
  align-content: center;
  cursor: default;
  width: 100%;
  height: 100%;
}

/* Expanded card animation */
.expanded-card {
  --offset-x: 0px;
  --offset-y: 0px;
  --card-delay: 0s;
  --card-width: calc(v-bind(EXPANDED_CARD_BASE_WIDTH) * 1px);
  --card-height: calc(v-bind(EXPANDED_CARD_BASE_HEIGHT) * 1px);
  
  width: var(--card-width) !important;
  height: var(--card-height) !important;
  animation: card-fly-in 0.5s cubic-bezier(0.34, 1.56, 0.64, 1) var(--card-delay) both;
}

.is-animating-out .expanded-card {
  animation: card-fly-out 0.35s ease-in var(--card-delay) both;
}

@keyframes card-fly-in {
  0% {
    transform: translate(var(--offset-x), var(--offset-y)) scale(v-bind(CARD_FLY_SCALE));
    opacity: 1;
  }
  100% {
    transform: translate(0, 0) scale(1);
    opacity: 1;
  }
}

@keyframes card-fly-out {
  0% {
    transform: translate(0, 0) scale(1);
    opacity: 1;
  }
  100% {
    transform: translate(var(--offset-x), var(--offset-y)) scale(v-bind(CARD_FLY_SCALE));
    opacity: 1;
  }
}

/* Individual card styling - full background photo design */
.format-card {
  width: calc(v-bind(STACK_CARD_WIDTH) * 1px);
  height: calc(v-bind(STACK_CARD_HEIGHT) * 1px);
  flex-shrink: 0;
  border: 1px solid var(--color-border-light);
  border-radius: var(--radius-sharp);
  padding: var(--space-md);
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  position: relative;
  overflow: hidden;
  transition: transform 0.3s ease, opacity 0.3s ease;
  background-color: var(--color-primary); /* Default opaque background for cards without images */
}

/* Wiggle animation for stacked cards on hover */
.format-card.is-stacked.is-wiggling {
  animation: wiggle 0.4s ease-in-out 2;
}

.format-card.is-stacked.is-wiggling:nth-child(2) {
  animation-delay: 0.05s;
}

.format-card.is-stacked.is-wiggling:nth-child(3) {
  animation-delay: 0.1s;
}

.format-card.is-stacked.is-wiggling:nth-child(4) {
  animation-delay: 0.15s;
}

.format-card.is-stacked.is-wiggling:nth-child(5) {
  animation-delay: 0.2s;
}

.format-card.is-stacked.is-wiggling:nth-child(6) {
  animation-delay: 0.25s;
}

@keyframes wiggle {
  0%, 100% {
    transform: translate(var(--card-offset-x), var(--card-offset-y)) rotate(var(--card-rotation));
  }
  25% {
    transform: translate(calc(var(--card-offset-x) + 2px), var(--card-offset-y)) rotate(calc(var(--card-rotation) + 1.5deg));
  }
  75% {
    transform: translate(calc(var(--card-offset-x) - 2px), var(--card-offset-y)) rotate(calc(var(--card-rotation) - 1.5deg));
  }
}

/* Explanation card with no background - inverted colors (green bg, white text) */
.format-card.no-background {
  background-color: var(--color-primary);
  justify-content: flex-end;
}

.format-card.no-background .card-title {
  color: white;
  text-shadow: none;
}

.format-card.no-background .card-description {
  color: white;
  text-shadow: none;
}

/* Full-card background photo */
.card-background {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  z-index: 0;
}

.card-background img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center;
}

/* Semi-transparent overlay for text readability - uses primary color */
.card-overlay {
  position: absolute;
  top: 0;
  left: 0;
  width: 100%;
  height: 100%;
  background: linear-gradient(to top, rgba(0, 0, 0, 0.7) 0%, rgba(0, 0, 0, 0.3) 60%, rgba(0, 0, 0, 0.1) 100%);
  z-index: 1;
}

/* Card content (overlaid on photo) */
.card-content {
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  gap: var(--space-sm);
  position: relative;
  z-index: 2;
}

/* Expanded cards have more space */
.format-card.expanded-card {
  cursor: default;
  padding: var(--space-lg);
}

.format-card.expanded-card .card-content {
  gap: var(--space-md);
}

.format-card.expanded-card .card-badge {
  padding: var(--space-sm) var(--space-lg);
  font-size: var(--text-xl);
}

.format-card.expanded-card .card-title {
  font-size: var(--text-lg);
}

.format-card.expanded-card .card-description {
  font-size: var(--text-sm);
  line-height: 1.4;
  -webkit-line-clamp: 3;
  line-clamp: 3;
}

/* Mobile: scroll snap */
.is-mobile .format-card {
  scroll-snap-align: center;
}

/* Mobile: smaller cards */
@media (max-width: 500px) {
  .is-mobile .format-card {
    width: 280px;
    height: 220px;
    padding: var(--space-sm);
  }
  
  .is-mobile .format-card .card-title {
    font-size: var(--text-sm);
  }
  
  .is-mobile .format-card .card-description {
    font-size: var(--text-xs);
    line-height: 1.35;
  }
  
  .is-mobile .format-card .card-badge {
    font-size: var(--text-base);
    padding: var(--space-xs) var(--space-sm);
  }
}

/* Very small screens */
@media (max-width: 380px) {
  .is-mobile .format-card {
    width: 240px;
    height: 190px;
  }
}

/* Mobile: show full description text */
.is-mobile .format-card .card-description {
  -webkit-line-clamp: unset;
  line-clamp: unset;
  display: block;
  overflow: visible;
}

/* Type badge - pill style matching existing category badges */
.card-badge {
  display: inline-flex;
  align-items: center;
  padding: var(--space-sm) var(--space-md);
  border-radius: var(--radius-pill);
  font-size: var(--text-lg);
  font-weight: var(--font-weight-semibold);
  white-space: nowrap;
}

.category-workshop {
  background-color: var(--category-workshop-bg);
  color: var(--category-workshop-text);
}

.category-vortrag {
  background-color: var(--category-vortrag-bg);
  color: var(--category-vortrag-text);
}

.category-sport {
  background-color: var(--category-sport-bg);
  color: var(--category-sport-text);
}

.category-diskussion {
  background-color: var(--category-diskussion-bg);
  color: var(--category-diskussion-text);
}

.category-sonstiges {
  background-color: var(--category-sonstiges-bg);
  color: var(--category-sonstiges-text);
}

/* Card title - color white for visibility on photos */
.card-title {
  font-size: var(--text-base);
  font-weight: var(--font-weight-semibold);
  margin: 0;
  color: white;
  text-shadow: 0 1px 3px rgba(0, 0, 0, 0.5);
}

.card-title.title-workshop,
.card-title.title-vortrag,
.card-title.title-sport,
.card-title.title-diskussion,
.card-title.title-sonstiges {
  color: white;
}

/* Card description */
.card-description {
  font-size: var(--text-sm);
  color: rgba(255, 255, 255, 0.95);
  line-height: 1.4;
  margin: 0;
  margin-bottom: var(--space-xs);
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
}

/* Hint text below the stack */
.stack-hint {
  position: absolute;
  bottom: -32px;
  left: 0;
  font-size: var(--text-xs);
  color: var(--color-main-text);
  opacity: 0.6;
  transition: opacity 0.3s ease;
}

.stack-container:hover .stack-hint {
  opacity: 0;
}
</style>
