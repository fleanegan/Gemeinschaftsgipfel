/**
 * Formats a date-time string into a localized German format
 * @param dateTimeString - ISO date string or any valid date string
 * @returns Formatted date string in German locale (e.g., "26. November 2025, 14:30")
 */
export function formatDateTime(dateTimeString: string): string {
  const date = new Date(dateTimeString);
  return new Intl.DateTimeFormat('de-DE', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(date);
}
