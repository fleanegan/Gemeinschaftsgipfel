import { describe, it, expect } from 'vitest';
import { mount } from '@vue/test-utils';
import InstructionCards from '../InstructionCards.vue';

describe('InstructionCards', () => {
  const mockInstructions = [
    {
      title: 'First Step',
      content: 'This is the first instruction'
    },
    {
      title: 'Second Step',
      content: 'This is the second instruction'
    },
    {
      title: 'Third Step',
      content: 'This is the third instruction'
    }
  ];

  it('renders all instruction cards', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: mockInstructions
      }
    });

    expect(wrapper.findAll('.instruction_card')).toHaveLength(3);
  });

  it('renders instruction titles correctly', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: mockInstructions
      }
    });

    const titles = wrapper.findAll('.instruction_card_content_header_title');
    expect(titles[0].text()).toBe('First Step');
    expect(titles[1].text()).toBe('Second Step');
    expect(titles[2].text()).toBe('Third Step');
  });

  it('renders instruction content correctly', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: mockInstructions
      }
    });

    expect(wrapper.text()).toContain('This is the first instruction');
    expect(wrapper.text()).toContain('This is the second instruction');
    expect(wrapper.text()).toContain('This is the third instruction');
  });

  it('renders enumerators in correct order', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: mockInstructions
      }
    });

    const enumerators = wrapper.findAll('.instruction_card_enumerator');
    expect(enumerators[0].text()).toBe('1.');
    expect(enumerators[1].text()).toBe('2.');
    expect(enumerators[2].text()).toBe('3.');
  });

  it('handles empty instructions array', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: []
      }
    });

    expect(wrapper.findAll('.instruction_card')).toHaveLength(0);
  });

  it('handles single instruction', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: [mockInstructions[0]]
      }
    });

    expect(wrapper.findAll('.instruction_card')).toHaveLength(1);
    expect(wrapper.text()).toContain('First Step');
    expect(wrapper.find('.instruction_card_enumerator').text()).toBe('1.');
  });

  it('renders instruction container', () => {
    const wrapper = mount(InstructionCards, {
      props: {
        instructions: mockInstructions
      }
    });

    expect(wrapper.find('.instruction_container').exists()).toBe(true);
  });
});
