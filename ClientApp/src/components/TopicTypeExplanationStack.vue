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
        :class="`format-${format.type.toLowerCase()}`"
        :style="{ opacity: cardOpacities[index] }"
      >
        <div class="card-background">
          <img :src="format.photo" :alt="format.type" />
        </div>
        <div class="card-overlay"></div>
        <div class="card-content">
          <span class="card-badge" :class="`category-${format.type.toLowerCase()}`">{{ format.type }}</span>
          <h3 class="card-title" :class="`title-${format.type.toLowerCase()}`">{{ format.title }}</h3>
          <p class="card-description">{{ format.description }}</p>
        </div>
      </div>
    </div>

    <!-- Desktop: stacked layout (collapsed state) -->
    <div v-else class="stack-wrapper">
      <div
        v-for="(format, index) in formats"
        :key="format.type"
        class="format-card is-stacked"
        :class="`format-${format.type.toLowerCase()}`"
        :style="getStackedCardStyle(index)"
      >
        <div class="card-background">
          <img :src="format.photo" :alt="format.type" />
        </div>
        <div class="card-overlay"></div>
        <div class="card-content">
          <span class="card-badge" :class="`category-${format.type.toLowerCase()}`">{{ format.type }}</span>
          <h3 class="card-title" :class="`title-${format.type.toLowerCase()}`">{{ format.title }}</h3>
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
          :class="`format-${format.type.toLowerCase()}`"
          :style="getExpandedCardStyle(index)"
          @click.stop
        >
          <div class="card-background">
            <img :src="format.photo" :alt="format.type" />
          </div>
          <div class="card-overlay"></div>
          <div class="card-content">
            <span class="card-badge" :class="`category-${format.type.toLowerCase()}`">{{ format.type }}</span>
            <h3 class="card-title" :class="`title-${format.type.toLowerCase()}`">{{ format.title }}</h3>
            <p class="card-description">{{ format.description }}</p>
          </div>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue';

type TopicType = 'Workshop' | 'Vortrag' | 'Sport' | 'Diskussion' | 'Sonstiges';

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
  'Workshop': '/photos/workshop.jpg',
  'Vortrag': '/photos/presentation.png',
  'Sport': '/photos/sport.jpg',
  'Diskussion': '/photos/discussion.jpg',
  'Sonstiges': '/photos/misc.jpg',
};

const baseFormats: { type: TopicType; title: string; description: string }[] = [
  { type: 'Workshop', title: 'Mitmach-Session', description: 'Interaktive Sessions zum gemeinsamen Lernen und Ausprobieren.' },
  { type: 'Vortrag', title: 'Präsentation', description: 'Klassische Vorträge zu spannenden Themen.' },
  { type: 'Sport', title: 'Bewegung', description: 'Gemeinsame sportliche Aktivitäten und Spiele.' },
  { type: 'Diskussion', title: 'Austausch', description: 'Offene Gesprächsrunden zu kontroversen oder interessanten Themen.' },
  { type: 'Sonstiges', title: 'Kreativ', description: 'Alles, was in keine andere Kategorie passt.' },
];

const formats: FormatInfo[] = baseFormats.map((format, index) => {
  const photo = photoMap[format.type];
  // Generate random offsets for each card (except the top one which stays neat)
  if (index === 0) {
    return { ...format, photo, offsetX: 0, offsetY: 0, rotation: 0 };
  }
  const seed = index * 42; // Consistent seed per card
  return {
    ...format,
    photo,
    offsetX: (seededRandom(seed) - 0.5) * 16, // -8 to +8 px
    offsetY: (seededRandom(seed + 1) - 0.5) * 12, // -6 to +6 px
    rotation: (seededRandom(seed + 2) - 0.5) * 8, // -4 to +4 degrees
  };
});

const containerRef = ref<HTMLElement | null>(null);
const scrollContainerRef = ref<HTMLElement | null>(null);

const isExpanded = ref(false);
const isAnimatingIn = ref(false);
const isAnimatingOut = ref(false);
const isMobile = ref(false);
const viewportWidth = ref(window.innerWidth);
const viewportHeight = ref(window.innerHeight);
const cardOpacities = ref<number[]>([1, 1, 1, 1, 1]);
const stackPosition = ref({ x: 0, y: 0 });

// Stacked card dimensions (small)
const STACK_CARD_WIDTH = 280;
const STACK_CARD_HEIGHT = 140;

const MOBILE_BREAKPOINT = 785;

// Base stack offset (cards pile roughly on top of each other)
const BASE_OFFSET_X = 2;
const BASE_OFFSET_Y = 2;

// Detect portrait mode (taller than wide)
const isPortrait = computed(() => viewportHeight.value > viewportWidth.value);

// Calculate responsive expanded card dimensions based on viewport
const expandedCardSize = computed(() => {
  const vw = viewportWidth.value;
  const vh = viewportHeight.value;
  const paddingX = 48; // Horizontal padding (24px each side)
  const paddingY = 48; // Vertical padding (24px each side)
  const gap = 24; // Gap between cards
  
  // Portrait: 2 columns × 3 rows, Landscape: 3 columns × 2 rows
  const columns = isPortrait.value ? 2 : 3;
  const rows = isPortrait.value ? 3 : 2;
  
  // Calculate max card width based on column count
  const availableWidth = vw - paddingX;
  const maxWidthFromViewport = (availableWidth - gap * (columns - 1)) / columns;
  
  // Calculate max card height based on row count
  const availableHeight = vh - paddingY;
  const maxHeightFromViewport = (availableHeight - gap * (rows - 1)) / rows;
  
  // Maintain aspect ratio (380:200 = 1.9)
  const aspectRatio = 380 / 200;
  
  // Start with width-based calculation
  let cardWidth = maxWidthFromViewport;
  let cardHeight = cardWidth / aspectRatio;
  
  // If height exceeds available space, recalculate based on height
  if (cardHeight > maxHeightFromViewport) {
    cardHeight = maxHeightFromViewport;
    cardWidth = cardHeight * aspectRatio;
  }
  
  // Apply reasonable min/max constraints
  cardWidth = Math.min(Math.max(cardWidth, 240), 500);
  cardHeight = Math.min(Math.max(cardHeight, 130), 280);
  
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
    transform: `translate(${totalOffsetX}px, ${totalOffsetY}px) rotate(${rotation}deg)`,
    zIndex: formats.length - index,
    opacity: index === 0 ? 1 : Math.max(0.5, 1 - index * 0.12),
  };
}

function getExpandedCardStyle(index: number): Record<string, string | number> {
  const delay = index * 0.05; // Stagger animation
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
    // Row 0: cards 0, 1
    // Row 1: cards 2, 3
    // Row 2: card 4 (centered)
    const row = Math.floor(index / 2);
    const col = index % 2;
    
    if (index === 4) {
      // Last card is centered in the third row
      cardGridX = gridStartX + (gridWidth - cardWidth) / 2;
      cardGridY = gridStartY + 2 * (cardHeight + gap);
    } else {
      cardGridX = gridStartX + col * (cardWidth + gap);
      cardGridY = gridStartY + row * (cardHeight + gap);
    }
  } else {
    // Landscape mode: 3 columns × 2 rows
    if (index < 3) {
      // First row
      cardGridX = gridStartX + index * (cardWidth + gap);
      cardGridY = gridStartY;
    } else {
      // Second row - centered (cards 4 and 5)
      const secondRowOffset = (cardWidth + gap) / 2;
      cardGridX = gridStartX + secondRowOffset + (index - 3) * (cardWidth + gap);
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
    stackPosition.value = { x: rect.left + 8, y: rect.top + 8 };
  }
  isExpanded.value = true;
  isAnimatingIn.value = true;
  setTimeout(() => {
    isAnimatingIn.value = false;
  }, 600);
}

function closeExpanded() {
  if (!isExpanded.value || isAnimatingOut.value) return;
  
  isAnimatingOut.value = true;
  setTimeout(() => {
    isExpanded.value = false;
    isAnimatingOut.value = false;
  }, 400);
}

function handleMouseEnter() {
  if (!isMobile.value && !isExpanded.value) {
    openExpanded();
  }
}

function handleMouseLeave() {
  // Don't close on mouse leave from stack - only close when leaving overlay
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
  
  // Apply opacity: 0.3 when scrolled out, 1.0 when visible
  cardOpacities.value = formats.map((_, index) => {
    if (index === 0) {
      return 0.3 + 0.7 * firstCardVisibility;
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
  margin-top: 1rem; /* Align with instruction card content (accounting for enumerator offset) */
}

/* Desktop: Size for compact stacked cards with some extra room for random offsets */
.stack-container:not(.is-mobile) {
  width: calc(280px + 20px); /* Card width + buffer for random offsets */
  height: calc(140px + 20px); /* Card height + buffer for random offsets */
  min-width: 0;
}

/* Mobile scroll container */
.scroll-container {
  display: flex;
  gap: 0.75rem;
  overflow-x: auto;
  scroll-snap-type: x mandatory;
  -webkit-overflow-scrolling: touch;
  scrollbar-width: none;
  padding: 0.5rem 0;
}

.scroll-container::-webkit-scrollbar {
  display: none;
}

/* Desktop stack wrapper */
.stack-wrapper {
  position: relative;
  margin-left: 8px;
  margin-top: 8px;
  width: 280px;
  height: 140px;
}

/* Fullscreen overlay */
.expanded-overlay {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  height: 100vh;
  background-color: rgba(0, 0, 0, 0.5);
  z-index: 10000;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
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
  --grid-card-width: 380px;
  --grid-card-height: 200px;
  --grid-gap: 24px;
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

/* Landscape mode: Center the bottom row (2 cards span middle columns) */
@media (orientation: landscape) {
  .expanded-grid .format-card:nth-child(4) {
    grid-column: 1 / 2;
    justify-self: end;
  }

  .expanded-grid .format-card:nth-child(5) {
    grid-column: 2 / 3;
    justify-self: start;
  }
}

/* Portrait mode: Center the last card in row 3 */
@media (orientation: portrait) {
  .expanded-grid .format-card:nth-child(5) {
    grid-column: 1 / -1;
    justify-self: center;
  }
}

/* Expanded card animation */
.expanded-card {
  --offset-x: 0px;
  --offset-y: 0px;
  --card-delay: 0s;
  --card-width: 380px;
  --card-height: 200px;
  
  width: var(--card-width) !important;
  height: var(--card-height) !important;
  animation: card-fly-in 0.5s cubic-bezier(0.34, 1.56, 0.64, 1) var(--card-delay) both;
}

.is-animating-out .expanded-card {
  animation: card-fly-out 0.35s ease-in var(--card-delay) both;
}

@keyframes card-fly-in {
  0% {
    transform: translate(var(--offset-x), var(--offset-y)) scale(0.68);
    opacity: 0;
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
    transform: translate(var(--offset-x), var(--offset-y)) scale(0.68);
    opacity: 0;
  }
}

/* Individual card styling - full background photo design */
.format-card {
  width: 280px;
  height: 140px;
  flex-shrink: 0;
  border: 1px solid var(--color-border-light);
  border-radius: 6px;
  padding: 1rem;
  box-sizing: border-box;
  display: flex;
  flex-direction: column;
  justify-content: flex-end;
  position: relative;
  overflow: hidden;
  transition: transform 0.3s ease, opacity 0.3s ease;
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

/* Semi-transparent overlay for text readability */
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
  gap: 0.35rem;
  position: relative;
  z-index: 2;
}

/* Expanded cards have more space */
.format-card.expanded-card {
  cursor: default;
  padding: 1.25rem;
}

.format-card.expanded-card .card-badge {
  padding: 0.25rem 0.6rem;
  font-size: 0.8rem;
}

.format-card.expanded-card .card-title {
  font-size: 1.1rem;
}

.format-card.expanded-card .card-description {
  font-size: 0.9rem;
  line-height: 1.4;
  -webkit-line-clamp: 3;
  line-clamp: 3;
}

/* Mobile: scroll snap */
.is-mobile .format-card {
  scroll-snap-align: start;
}

/* Type badge - pill style matching existing category badges */
.card-badge {
  display: inline-flex;
  align-items: center;
  padding: 0.2rem 0.5rem;
  border-radius: 10px;
  font-size: 0.7rem;
  font-weight: 600;
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
  font-size: 0.95rem;
  font-weight: 600;
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
  font-size: 0.8rem;
  color: rgba(255, 255, 255, 0.95);
  line-height: 1.4;
  margin: 0;
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
  bottom: -24px;
  left: 0;
  font-size: 0.75rem;
  color: var(--color-main-text);
  opacity: 0.6;
  transition: opacity 0.3s ease;
}

.stack-container:hover .stack-hint {
  opacity: 0;
}
</style>
