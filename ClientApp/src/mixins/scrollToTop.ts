/**
 * Mixin for scroll-to-top functionality
 * Provides a sticky button that appears when scrolling down
 */
export const scrollToTopMixin = {
  data() {
    return {
      isSticky: false,
      scrollThreshold: 750, // Can be overridden in component
    };
  },
  methods: {
    handleScroll(): void {
      (this as any).isSticky = window.scrollY > (this as any).scrollThreshold;
    },
    scrollToTop(): void {
      scrollTo(0, 0);
    },
  },
  mounted() {
    window.addEventListener('scroll', (this as any).handleScroll);
  },
  beforeUnmount() {
    window.removeEventListener('scroll', (this as any).handleScroll);
  },
};
