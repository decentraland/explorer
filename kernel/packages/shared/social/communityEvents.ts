import { defaultLogger } from 'shared/logger'

const DEFAULT_FETCH_INTERVAL = 60000

let fetchIntervalTime: number = DEFAULT_FETCH_INTERVAL
let lastFetchTime: number = 0

export function initCommunityEvents() {
  let sentInitialMessage = false

  setInterval(() => {
    const now = Date.now()
    if (now - lastFetchTime > fetchIntervalTime) {
      fetchEvents()
        .then((events) => {
          if (events !== null) {
            lastFetchTime = now

            if (!sentInitialMessage) {
              sentInitialMessage = true
              for (let live of events.liveId) {
                console.log(`PATO: live event ${events.all[live].name}`)
              }
            }
            fetchIntervalTime = now + events.interval
          }
        })
        .catch()
    }
  }, 1000)
}

async function fetchEvents(): Promise<Events | null> {
  try {
    const response = await fetch(`https://events.decentraland.org/api/events`)
    const json = (await response.json()) as EventApiResponse
    return processEvents(json.data)
  } catch (e) {
    defaultLogger.error(e)
  }
  return null
}

function processEvents(events: EventJsonData[]): Events {
  let result: Events = {
    liveId: [],
    todayId: [],
    all: {},
    interval: DEFAULT_FETCH_INTERVAL
  }

  const today = new Date()

  for (let event of events) {
    const startDate = new Date(event.next_start_at)

    if (event.live) {
      result.liveId.push(event.id)
      if (!event.all_day) {
        const millisecondsLeft = startDate.getTime() + event.duration - startDate.getTime()
        if (millisecondsLeft > 0 && millisecondsLeft < result.interval) {
          result.interval = millisecondsLeft
        }
      }
    } else if (isToday(today, startDate)) {
      result.todayId.push(event.id)
      const millisecondsToStart = startDate.getTime() - today.getTime()
      if (millisecondsToStart > 0 && millisecondsToStart < result.interval) {
        result.interval = millisecondsToStart
      }
    }
    result.all[event.id] = event
  }
  return result
}

function isToday(now: Date, date: Date): boolean {
  return (
    date.getDate() === now.getDate() && date.getMonth() === now.getMonth() && date.getFullYear() === now.getFullYear()
  )
}

type EventApiResponse = {
  ok: boolean
  data: EventJsonData[]
}

type EventJsonData = {
  id: string
  name: string
  image: string
  description: string
  next_start_at: string
  position: number[]
  live: boolean
  duration: number
  all_day: boolean
}

type Events = {
  liveId: string[]
  todayId: string[]
  all: Record<string, EventJsonData>
  interval: number
}
