import React, { useState } from "react";
import "./PassportForm.css";

export interface PassportFormProps {
  name?: string;
  email?: string;
  onSubmit: (name: string, email: string) => void;
}

export const PassportForm: React.FC<PassportFormProps> = (props) => {
  const [name, setName] = useState(props.name || "");
  const [email, setEmail] = useState(props.email || "");
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    props.onSubmit(name, email);
  };

  return (
    <div className="passportForm">
      <form method="POST" onSubmit={handleSubmit}>
        {/*<em>*required field (you can edit it later)</em>*/}
        <div className="inputGroup">
          <label>Name your avatar</label>
          <input
            type="text"
            name="name"
            placeholder="your avatar name"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />
        </div>
        <div className="inputGroup">
          <label>Let's stay in touch</label>
          <input
            type="text"
            name="email"
            placeholder="enter your email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </div>
        <div className="actions">
          <button type="submit" className="btnSubmit">
            NEXT
          </button>
        </div>
      </form>
    </div>
  );
};
