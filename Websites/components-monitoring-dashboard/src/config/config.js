export async function getConfig() {
  const res = await fetch('/config.json');
  return res.json();
}