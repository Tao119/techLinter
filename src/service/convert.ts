export function convertToSnakeCase(obj: any): any {
  if (obj === null || typeof obj !== "object") {
    return obj;
  }

  if (Array.isArray(obj)) {
    return obj.map((item) => convertToSnakeCase(item));
  }

  const snakeObj: any = {};
  for (const [key, value] of Object.entries(obj)) {
    const snakeKey = key.replace(
      /[A-Z]/g,
      (match) => `_${match.toLowerCase()}`
    );
    snakeObj[snakeKey] = convertToSnakeCase(value);
  }

  return snakeObj;
}

export function convertToCamelCase(obj: any): any {
  if (obj === null || typeof obj !== "object") {
    return obj;
  }

  if (Array.isArray(obj)) {
    return obj.map((item) => convertToCamelCase(item));
  }

  const camelObj: any = {};
  for (const [key, value] of Object.entries(obj)) {
    const camelKey = key
      .replace(/_([a-z])/g, (match, group1) => group1.toUpperCase())
      .replace(/^[A-Z]/, (match) => match.toLowerCase());
    camelObj[camelKey] = convertToCamelCase(value);
  }

  return camelObj;
}

