export const projectStatusLabels = {
  planned: 'Tervezett',
  in_progress: 'Folyamatban',
  paused: 'Szüneteltetett',
  completed: 'Befejezett',
  cancelled: 'Törölve'
}

export const priorityLabels = {
  low: 'Alacsony',
  medium: 'Közepes',
  high: 'Magas'
}

export const jobStatusLabels = {
  applied: 'Jelentkezve',
  waiting_response: 'Válaszra vár',
  interview_scheduled: 'Interjú egyeztetve',
  first_interview: 'Első interjú',
  second_interview: 'Második interjú',
  offer_received: 'Ajánlatot kaptam',
  rejected: 'Elutasítva',
  declined: 'Visszautasítottam',
  closed: 'Lezárva'
}

export const eventTypeLabels = {
  project: 'Projekt',
  task: 'Feladat',
  interview: 'Interjú',
  personal: 'Személyes',
  deadline: 'Határidő',
  other: 'Egyéb'
}

export const taskStatusLabels = {
  todo: 'Teendő',
  in_progress: 'Folyamatban',
  done: 'Kész',
  cancelled: 'Törölve'
}

export const projectStatusOptions = Object.entries(projectStatusLabels).map(([value, label]) => ({ value, label }))
export const priorityOptions = Object.entries(priorityLabels).map(([value, label]) => ({ value, label }))
export const jobStatusOptions = Object.entries(jobStatusLabels).map(([value, label]) => ({ value, label }))
export const eventTypeOptions = Object.entries(eventTypeLabels).map(([value, label]) => ({ value, label }))
export const taskStatusOptions = Object.entries(taskStatusLabels).map(([value, label]) => ({ value, label }))

export function labelFor(value) {
  return jobStatusLabels[value]
    || projectStatusLabels[value]
    || priorityLabels[value]
    || eventTypeLabels[value]
    || taskStatusLabels[value]
    || value
}
