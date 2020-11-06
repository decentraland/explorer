import React, { useState, useRef, useMemo } from "react";
import "./Accordion.css";

export interface AccordionProps {
  title?: React.ReactNode
  children?: React.ReactNode
  open?: boolean
}

export function Accordion(props: AccordionProps) {

  const [ open, setOpen ] = useState(false)
  const ref = useRef<HTMLDivElement>(null)
  const  height = useMemo(() => {
    if (!ref.current || !open) {
      return 0
    }

    return ref.current.offsetHeight + 50
  }, [ open, ref.current !== null ])

  return <div className={'eth-accordion' + (open && ' eth-accordion-open' || '')}>
    <div className="eth-accordion-title" onClick={() => setOpen(!open)}>
      {props.title}
    </div>
    <div className="eth-accordion-description" style={{ height: `${height}px` }}>
      <div ref={ref}>
        {props.children}
      </div>
    </div>
  </div>
}