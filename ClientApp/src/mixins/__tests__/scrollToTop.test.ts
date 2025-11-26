import { describe, it, expect, beforeEach, vi } from 'vitest';
import { mount } from '@vue/test-utils';
import { defineComponent } from 'vue';
import { scrollToTopMixin } from '../scrollToTop';

// Create a test component that uses the mixin
const TestComponent = defineComponent({
  mixins: [scrollToTopMixin],
  template: `
    <div>
      <div :class="{'visible': isSticky}">Scroll to top button</div>
      <button @click="scrollToTop">Top</button>
    </div>
  `
});

describe('scrollToTopMixin', () => {
  beforeEach(() => {
    // Reset scroll position
    window.scrollY = 0;
    // Mock scrollTo
    window.scrollTo = vi.fn();
  });

  it('initializes with isSticky as false', () => {
    const wrapper = mount(TestComponent);
    expect(wrapper.vm.isSticky).toBe(false);
  });

  it('sets default scrollThreshold to 750', () => {
    const wrapper = mount(TestComponent);
    expect(wrapper.vm.scrollThreshold).toBe(750);
  });

  it('updates isSticky when scrolling past threshold', () => {
    const wrapper = mount(TestComponent);
    
    // Simulate scrolling past threshold
    Object.defineProperty(window, 'scrollY', { value: 800, writable: true });
    wrapper.vm.handleScroll();
    
    expect(wrapper.vm.isSticky).toBe(true);
  });

  it('keeps isSticky false when below threshold', () => {
    const wrapper = mount(TestComponent);
    
    // Simulate scrolling below threshold
    Object.defineProperty(window, 'scrollY', { value: 500, writable: true });
    wrapper.vm.handleScroll();
    
    expect(wrapper.vm.isSticky).toBe(false);
  });

  it('scrolls to top when scrollToTop is called', () => {
    const wrapper = mount(TestComponent);
    wrapper.vm.scrollToTop();
    
    expect(window.scrollTo).toHaveBeenCalledWith(0, 0);
  });

  it('adds scroll event listener on mount', () => {
    const addEventListenerSpy = vi.spyOn(window, 'addEventListener');
    mount(TestComponent);
    
    expect(addEventListenerSpy).toHaveBeenCalledWith('scroll', expect.any(Function));
  });

  it('removes scroll event listener on unmount', () => {
    const removeEventListenerSpy = vi.spyOn(window, 'removeEventListener');
    const wrapper = mount(TestComponent);
    wrapper.unmount();
    
    expect(removeEventListenerSpy).toHaveBeenCalledWith('scroll', expect.any(Function));
  });

  it('allows overriding scrollThreshold in component', () => {
    const CustomThresholdComponent = defineComponent({
      mixins: [scrollToTopMixin],
      data() {
        return {
          scrollThreshold: 1000, // Override default
        };
      },
      template: '<div>Test</div>'
    });

    const wrapper = mount(CustomThresholdComponent);
    expect(wrapper.vm.scrollThreshold).toBe(1000);
  });
});
