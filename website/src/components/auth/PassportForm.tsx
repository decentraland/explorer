import React, { useState } from "react";

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
    <form id="signup-form" method="POST" onSubmit={handleSubmit}>
      <div>
        <div>*required field (you can edit it later)</div>
        <label>Name your avatar</label>
        <input
          id="signup-avatar-name"
          type="text"
          name="name"
          placeholder="your avatar name"
          value={name}
          onChange={(e) => setName(e.target.value)}
        />
      </div>
      <div>
        <label>Let's stay in touch</label>
        <input
          id="signup-avatar-email"
          type="text"
          name="email"
          placeholder="enter your email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
        />
      </div>
      <div>
        <button type="submit" className="button primary center">
          Next
        </button>
      </div>
    </form>
  );
};
