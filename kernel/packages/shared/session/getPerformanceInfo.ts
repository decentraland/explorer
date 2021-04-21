export function getPerformanceInfo(data: { samples: string; fpsIsCapped: boolean, hiccupsInThousandFrames: number; hiccupsTime: number; totalTime: number }) {
  const entries: number[] = []
  const length = data.samples.length
  let sum = 0
  for (let i = 0; i < length; i++) {
    entries[i] = data.samples.charCodeAt(i)
    sum += entries[i]
  }
  const sorted = entries.sort((a, b) => a - b)

  return {
    idle: document.hidden,
    fps: (1000 * length) / sum,
    avg: sum / length,
    total: sum,
    len: length,
    min: sorted[0],
    p1: sorted[Math.ceil(length * 0.01)],
    p5: sorted[Math.ceil(length * 0.05)],
    p10: sorted[Math.ceil(length * 0.10)],
    p20: sorted[Math.ceil(length * 0.20)],
    p50: sorted[Math.ceil(length * 0.50)],
    p75: sorted[Math.ceil(length * 0.75)],
    p80: sorted[Math.ceil(length * 0.80)],
    p90: sorted[Math.ceil(length * 0.90)],
    p95: sorted[Math.ceil(length * 0.95)],
    p99: sorted[Math.ceil(length * 0.99)],
    max: sorted[length - 1],
    capped: data.fpsIsCapped,
    hiccupsInThousandFrames: data.hiccupsInThousandFrames,
    hiccupsTime: data.hiccupsTime,
    totalTime: data.totalTime
  }
}
