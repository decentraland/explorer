import { defaultLogger } from 'shared/logger'
import { notifyStatusThroughChat } from '../comms/chat'

const DEFAULT_FETCH_INTERVAL = 60000

let fetchIntervalTime: number = DEFAULT_FETCH_INTERVAL
let lastFetchTime: number = 0
let initialMessageSent = false
let initialized = false
let liveEventsReported: string[] = []

export function initCommunityEvents() {
  if (initialized) {
    return
  }

  initialMessageSent = false
  initialized = true
  fetchAndReportEvents(Date.now())
  setInterval(eventsInterval, 30000)
}

function eventsInterval() {
  const now = Date.now()
  if (now - lastFetchTime > fetchIntervalTime) {
    fetchAndReportEvents(now)
  }
}

function sendInitialMessage(events: Events) {
  if (initialMessageSent) {
    return
  }

  initialMessageSent = true

  if (events.liveId.length > 0 || events.todayId.length > 0) {
    let message: string = ''
    for (let eventId of events.liveId) {
      liveEventsReported.push(eventId)
      message += formatLiveEventString(events.all[eventId])
    }
    for (let eventId of events.todayId) {
      message += formatTodayEventString(events.all[eventId])
    }

    if (message !== '') {
      notifyStatusThroughChat(message)
    }
  }
}

function formatLiveEventString(event: EventJsonData) {
  return `<u>Event now</u>: <i>${event.name}</i> ` + `<nobr>@ ${event.position[0]},${event.position[1]}</nobr>\n`
}

function formatTodayEventString(event: EventJsonData) {
  let date = new Date(event.next_start_at)
  return `<u>Event today</u>: <i>${event.name}</i> <nobr>@ ${event.position[0]},${
    event.position[1]
  }</nobr> <nobr>${date.getHours()}:${date
    .getMinutes()
    .toLocaleString(undefined, { minimumIntegerDigits: 2 })}hrs</nobr>\n`
}

function reportUnreportedLiveEvents(events: Events) {
  const unreportedLiveEvents = events.liveId.filter(
    (eventId) => liveEventsReported.filter((id) => id !== eventId).length === 0
  )
  let message: string = ''
  for (let eventId of unreportedLiveEvents) {
    liveEventsReported.push(eventId)
    message += formatLiveEventString(events.all[eventId])
  }
  if (message !== '') {
    notifyStatusThroughChat(message)
  }
}

function fetchAndReportEvents(now: number) {
  fetchEvents()
    .then((events) => {
      if (events !== null) {
        lastFetchTime = now

        if (!initialMessageSent) {
          sendInitialMessage(events)
        }

        reportUnreportedLiveEvents(events)

        fetchIntervalTime = events.interval
      }
    })
    .catch()
}

async function fetchEvents(): Promise<Events | null> {
  try {
    const response = await fetch(`https://events.decentraland.org/api/events`)
    const json = (await response.json()) as EventApiResponse
    if (Array.isArray(json.data)) {
      return processEvents(json.data)
    }
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

  const date = new Date()

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
    } else if (isToday(date, startDate)) {
      result.todayId.push(event.id)
      const millisecondsToStart = startDate.getTime() - date.getTime()
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
