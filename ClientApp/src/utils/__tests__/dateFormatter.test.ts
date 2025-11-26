import { describe, it, expect } from 'vitest';
import { formatDateTime } from '../dateFormatter';

describe('formatDateTime', () => {
  it('formats a valid ISO date string correctly', () => {
    const isoDate = '2025-11-26T14:30:00Z';
    const result = formatDateTime(isoDate);
    
    // Result should contain the date in German format
    expect(result).toContain('26');
    expect(result).toContain('November');
    expect(result).toContain('2025');
    expect(result).toMatch(/\d{2}:\d{2}/); // Should contain time in HH:MM format
  });

  it('formats a date with single-digit day correctly', () => {
    const isoDate = '2025-01-05T09:15:00Z';
    const result = formatDateTime(isoDate);
    
    expect(result).toContain('5');
    expect(result).toContain('Januar');
    expect(result).toContain('2025');
  });

  it('formats midnight time correctly', () => {
    const isoDate = '2025-12-31T00:00:00Z';
    const result = formatDateTime(isoDate);
    
    expect(result).toContain('31');
    expect(result).toContain('Dezember');
    expect(result).toContain('2025');
    expect(result).toMatch(/\d{2}:\d{2}/);
  });

  it('handles different date string formats', () => {
    // Test with a standard date string
    const dateString = '2025-06-15 10:30:00';
    const result = formatDateTime(dateString);
    
    expect(result).toContain('15');
    expect(result).toContain('Juni');
    expect(result).toContain('2025');
  });

  it('returns a string', () => {
    const isoDate = '2025-11-26T14:30:00Z';
    const result = formatDateTime(isoDate);
    
    expect(typeof result).toBe('string');
  });
});
