import React, { useState } from "react";
import "./PassportForm.css";

export interface PassportFormProps {
  name?: string;
  email?: string;
  onSubmit: (name: string, email: string) => void;
}

export const PassportForm: React.FC<PassportFormProps> = (props) => {
  const [chars, setChars] = useState(props.name ? props.name.length : 0);
  const [name, setName] = useState(props.name || "");
  const [email, setEmail] = useState(props.email || "");
  const [hasError, setHasError] = useState(false);
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!name || name.trim().length > 15) {
      setHasError(true);
      return;
    }
    props.onSubmit(name, email);
  };

  const onChangeName = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    if (target.value.length <= 15) {
      setHasError(false);
      setName(target.value);
      setChars(target.value.length);
    }
  };

  return (
    <div className="passportForm">
      <form method="POST" onSubmit={handleSubmit}>
        <div className="inputGroup">
          <label>Name your avatar</label>
          <input
            type="text"
            name="name"
            className={hasError ? "hasError" : ""}
            placeholder="your avatar name"
            value={name}
            onChange={onChangeName}
          />
          {chars > 0 && <em className="warningLength">{chars}/15</em>}
          {hasError && (
            <em className="error">*required field (you can edit it later)</em>
          )}
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
