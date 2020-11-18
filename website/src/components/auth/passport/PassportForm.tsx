import React, { useState, useMemo } from "react";
import "./PassportForm.css";

// eslint-disable-next-line
const emailPattern = /^((([a-z]|\d|[!#$%&'*+\-/=?^_`{|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+(\.([a-z]|\d|[!#$%&'*+\-/=?^_`{|}~]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])+)*)|((\x22)((((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(([\x01-\x08\x0b\x0c\x0e-\x1f\x7f]|\x21|[\x23-\x5b]|[\x5d-\x7e]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(\\([\x01-\x09\x0b\x0c\x0d-\x7f]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF]))))*(((\x20|\x09)*(\x0d\x0a))?(\x20|\x09)+)?(\x22)))@((([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|\d|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.)+(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])|(([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])([a-z]|\d|-|\.|_|~|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])*([a-z]|[\u00A0-\uD7FF\uF900-\uFDCF\uFDF0-\uFFEF])))\.?$/;

export interface PassportFormProps {
  name?: string;
  email?: string;
  onSubmit: (name: string, email: string) => void;
}

const MAX_NAME_LENGTH = 15

export const PassportForm: React.FC<PassportFormProps> = (props) => {
  const [name, setName] = useState(props.name || "");
  const [email, setEmail] = useState(props.email || "");
  const hasNameError = useMemo(() => name.length > MAX_NAME_LENGTH, [name])
  const hasEmailError = useMemo(() => email.trim().length > 0 && !emailPattern.test(email), [email])
  const handleSubmit = (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!name || name.trim().length > MAX_NAME_LENGTH) {
      return;
    }
    if (email.trim().length > 0 && !emailPattern.test(email)) {
      return;
    }

    props.onSubmit(name.trim(), email.trim());
  };

  const onChangeName = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    setName(target.value);
  };

  const onChangeEmail = ({ target }: React.ChangeEvent<HTMLInputElement>) => {
    setEmail(target.value);
  };

  return (
    <div className="passportForm">
      <form method="POST" onSubmit={handleSubmit}>
        <div className="inputGroup">
          <em className="required">* required field (you can edit it later)</em>
          <label>Name your avatar</label>
          <input
            type="text"
            name="name"
            className={hasNameError ? "hasError" : ""}
            placeholder="Your avatar name"
            value={name}
            onChange={onChangeName}
          />
          <em className={'hint' + (hasNameError ? ' hasError' : '')}>{Math.max(MAX_NAME_LENGTH - name.length, 0)}/{MAX_NAME_LENGTH}</em>
        </div>
        <div className="inputGroup">
          <label>Let's stay in touch</label>
          <input
            type="text"
            name="email"
            className={hasEmailError ? "hasError" : ""}
            placeholder="Enter your email"
            value={email}
            onChange={onChangeEmail}
          />
          <em className="hint hasError">{hasEmailError ? 'Enter a valid email' : ''}</em>
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
