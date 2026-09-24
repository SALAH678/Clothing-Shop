import React, { useState, type FormEvent } from "react";
import { X } from "lucide-react";
import { useCreateUser } from "../hooks/useAdminUserMutations";
import { getApiErrorMessage } from "../utils/apiErrors";
import {
  ALGERIAN_PHONE_PATTERN,
  digitsOnly,
  NAME_PATTERN,
  STRONG_PASSWORD_PATTERN,
} from "../../../lib/validation";

interface CreateUserFormProps {
  onClose: () => void;
  onCreated?: () => void;
}

const inputClass =
  "w-full border-2 border-primary bg-surface p-3 font-mono text-sm focus:outline-none focus:bg-primary focus:text-white transition-colors placeholder:text-secondary";

export const CreateUserForm: React.FC<CreateUserFormProps> = ({ onClose, onCreated }) => {
  const createUser = useCreateUser();

  const [firstName, setFirstName] = useState("");
  const [lastName, setLastName] = useState("");
  const [phoneNumber, setPhoneNumber] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [showPassword, setShowPassword] = useState(false);
  const [role, setRole] = useState<"Customer" | "Admin">("Customer");
  const [formError, setFormError] = useState<string | null>(null);

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    setFormError(null);

    const trimmedFirst = firstName.trim();
    const trimmedLast = lastName.trim();
    const trimmedPhone = phoneNumber.trim();
    const trimmedEmail = email.trim();

    if (!trimmedFirst || !NAME_PATTERN.test(trimmedFirst)) {
      setFormError("First name is required and must contain only letters and spaces.");
      return;
    }
    if (!trimmedLast || !NAME_PATTERN.test(trimmedLast)) {
      setFormError("Last name is required and must contain only letters and spaces.");
      return;
    }
    if (!trimmedEmail || !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(trimmedEmail)) {
      setFormError("A valid email address is required.");
      return;
    }
    if (!ALGERIAN_PHONE_PATTERN.test(trimmedPhone)) {
      setFormError("Phone number must be a valid Algerian mobile number (e.g. 0551234567).");
      return;
    }
    if (!STRONG_PASSWORD_PATTERN.test(password)) {
      setFormError(
        "Password must be at least 6 characters and contain an uppercase letter, a lowercase letter, a number, and a special character.",
      );
      return;
    }

    try {
      await createUser.mutateAsync({
        firstName: trimmedFirst,
        lastName: trimmedLast,
        phoneNumber: trimmedPhone,
        email: trimmedEmail,
        password,
        role,
      });
      onCreated?.();
      onClose();
    } catch (error) {
      setFormError(getApiErrorMessage(error, "Failed to create user."));
    }
  };

  return (
    <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
      <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-6 sm:p-8 w-full max-w-md relative max-h-[90vh] overflow-y-auto">
        <button
          type="button"
          onClick={onClose}
          className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all cursor-pointer"
          title="Close"
        >
          <X className="w-6 h-6" />
        </button>

        <h2 className="font-display text-3xl font-black uppercase tracking-tighter">Add User</h2>
        <p className="font-mono text-xs text-secondary mt-1 mb-6 uppercase">
          Create a customer or admin account.
        </p>

        {formError && (
          <div className="bg-red-50 border-4 border-red-600 p-3 mb-4 font-mono text-xs font-bold uppercase text-red-700">
            {formError}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
              First Name *
              <input
                required
                type="text"
                maxLength={100}
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder="e.g. John"
                className={inputClass}
              />
            </label>
            <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
              Last Name *
              <input
                required
                type="text"
                maxLength={100}
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                placeholder="e.g. Doe"
                className={inputClass}
              />
            </label>
          </div>

          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            Email *
            <input
              required
              type="email"
              maxLength={320}
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="e.g. john.doe@example.com"
              className={inputClass}
            />
          </label>

          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            Phone Number *
            <input
              required
              type="tel"
              inputMode="numeric"
              maxLength={10}
              value={phoneNumber}
              onChange={(e) => setPhoneNumber(digitsOnly(e.target.value))}
              placeholder="e.g. 0551234567"
              className={inputClass}
            />
          </label>

          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            Password *
            <input
              required
              type={showPassword ? "text" : "password"}
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              className={inputClass}
            />
          </label>
          <label className="flex items-center gap-2 font-mono text-xs font-bold uppercase cursor-pointer select-none">
            <input
              type="checkbox"
              checked={showPassword}
              onChange={(e) => setShowPassword(e.target.checked)}
              className="w-4 h-4 accent-primary"
            />
            Show password
          </label>

          <label className="flex flex-col gap-1.5 font-mono text-xs font-bold uppercase">
            Role *
            <select
              value={role}
              onChange={(e) => setRole(e.target.value as "Customer" | "Admin")}
              className={`${inputClass} cursor-pointer`}
            >
              <option value="Customer">Customer</option>
              <option value="Admin">Admin</option>
            </select>
          </label>

          <button
            type="submit"
            disabled={createUser.isPending}
            className="mt-2 w-full bg-primary text-white font-mono font-black uppercase py-3 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none"
          >
            {createUser.isPending ? "Creating…" : "Create User"}
          </button>
        </form>
      </div>
    </div>
  );
};

export default CreateUserForm;
