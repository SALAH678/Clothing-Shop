import { useState, useEffect } from "react";
import { User, CheckCircle, AlertCircle } from "lucide-react";
import { Link, useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import type { FieldValues } from "react-hook-form";
import { useCurrentUserProfile, useUpdateCurrentUserProfile } from "../features/users/hooks/useUser";
import { useLogout } from "../features/auth/hooks";
import type { ApiErrorResponse } from "../features/auth/types/ApiErrorResponse";

export default function Profile() {
  const navigate = useNavigate();
  const [isEditing, setIsEditing] = useState(false);
  const [updateSuccess, setUpdateSuccess] = useState(false);

  const { data: userProfile, isLoading, error: profileError } = useCurrentUserProfile();
  const { mutate: updateProfile, isPending: isUpdating, error: updateError } = useUpdateCurrentUserProfile();
  const { mutate: logoutUser, isPending: isLoggingOut } = useLogout();

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm({
    defaultValues: {
      firstName: "",
      lastName: "",
      phone: "",
    },
    mode: "onChange",
  });

  useEffect(() => {
    if (userProfile) {
      reset({
        firstName: userProfile.firstName || "",
        lastName: userProfile.lastName || "",
        phone: userProfile.phoneNumber || "",
      });
    }
  }, [userProfile, reset]);

  // `successTick` restarts the auto-hide timer on every successful save, so
  // repeated saves cannot stack up multiple pending timeouts, and the effect
  // cleanup cancels the timer on unmount.
  const [successTick, setSuccessTick] = useState(0);

  useEffect(() => {
    if (successTick === 0) return;
    const timer = setTimeout(() => setUpdateSuccess(false), 4000);
    return () => clearTimeout(timer);
  }, [successTick]);

  const onSubmit = (data: FieldValues) => {
    setUpdateSuccess(false);
    updateProfile(
      {
        firstName: data.firstName,
        lastName: data.lastName,
        phoneNumber: data.phone,
      },
      {
        onSuccess: () => {
          setIsEditing(false);
          setUpdateSuccess(true);
          setSuccessTick((tick) => tick + 1);
        },
      },
    );
  };

  const handleLogout = () => {
    logoutUser(undefined, {
      onSuccess: () => {
        navigate("/auth/login", { replace: true });
      },
    });
  };

  if (isLoading) {
    return (
      <div className="grow flex items-center justify-center py-20 bg-surface">
        <div className="font-mono text-sm font-bold uppercase animate-pulse">Loading profile...</div>
      </div>
    );
  }

  if (profileError) {
    return (
      <div className="grow flex items-center justify-center py-20 bg-surface">
        <div className="p-6 bg-red-50 border-4 border-red-500 font-mono text-sm font-bold text-red-600 uppercase">
          Failed to load profile. Please log in again.
        </div>
      </div>
    );
  }

  const currentUser = userProfile || {
    firstName: "",
    lastName: "",
    phoneNumber: "",
    email: "",
    isEmailVerified: false,
  };

  return (
    <div className="grow flex flex-col md:flex-row py-12 px-4 md:px-8 lg:px-16 gap-8 bg-surface">
      {/* Sidebar Navigation */}
      <aside className="w-full md:w-64 shrink-0">
        <div className="bg-white border-4 border-primary shadow-[4px_4px_0_0_#000] p-6 mb-8 relative">
          <div className="w-16 h-16 bg-primary text-white rounded-full flex items-center justify-center border-2 border-primary mb-4 absolute -top-8 left-6">
            <User className="w-8 h-8" />
          </div>
          <div className="mt-6">
            <h2 className="font-display text-xl font-black uppercase tracking-tighter line-clamp-1">
              {currentUser.firstName || "USER"}
            </h2>
            <p className="font-mono text-xs text-secondary font-bold break-all">{currentUser.email}</p>
            <div className="mt-3 flex items-center gap-1.5 font-mono text-[11px] font-bold uppercase">
              {currentUser.isEmailVerified ? (
                <span className="text-green-700 flex items-center gap-1">
                  <CheckCircle className="w-3.5 h-3.5" /> VERIFIED
                </span>
              ) : (
                <span className="text-amber-700 flex items-center gap-1">
                  <AlertCircle className="w-3.5 h-3.5" /> UNVERIFIED
                </span>
              )}
            </div>
          </div>
        </div>

        <nav className="flex flex-col gap-2">
          <Link
            to="/profile"
            className="font-mono text-sm uppercase font-bold tracking-widest bg-primary text-white border-2 border-primary py-3 px-4 shadow-[2px_2px_0_0_#000]"
          >
            USER INFORMATION
          </Link>
          <div className="h-px bg-zinc-200 my-2"></div>
          <button
            type="button"
            onClick={handleLogout}
            disabled={isLoggingOut}
            className="font-mono text-left text-sm uppercase font-bold tracking-widest text-red-500 hover:bg-red-50 py-3 px-4 transition-colors disabled:opacity-50"
          >
            {isLoggingOut ? "LOGGING OUT..." : "LOG OUT"}
          </button>
        </nav>
      </aside>

      {/* Main Content */}
      <main className="grow">
        <form
          onSubmit={handleSubmit(onSubmit)}
          className="bg-white border-4 border-primary p-6 md:p-10 shadow-[8px_8px_0_0_#000]"
        >
          <div className="border-b-4 border-primary pb-6 mb-8 flex justify-between items-end">
            <div>
              <h1 className="font-display text-3xl font-black tracking-tighter uppercase">USER INFORMATION</h1>
              <p className="font-mono text-sm text-secondary font-bold mt-2">
                Manage your profile details and contact information.
              </p>
            </div>
            {isEditing ? (
              <div className="hidden md:flex gap-4">
                <button
                  type="button"
                  onClick={() => setIsEditing(false)}
                  className="font-mono text-xs uppercase font-bold tracking-widest border-2 border-transparent px-4 py-2 hover:bg-surface-container-low transition-colors"
                >
                  CANCEL
                </button>
                <button
                  type="submit"
                  disabled={isUpdating}
                  className="font-mono text-xs uppercase font-bold tracking-widest border-2 border-primary px-4 py-2 bg-primary text-white hover:bg-white hover:text-black hover:shadow-[4px_4px_0_0_#000] transition-all disabled:opacity-50"
                >
                  {isUpdating ? "SAVING..." : "SAVE"}
                </button>
              </div>
            ) : (
              <button
                type="button"
                onClick={() => setIsEditing(true)}
                className="hidden md:block font-mono text-xs uppercase font-bold tracking-widest border-2 border-primary px-4 py-2 hover:bg-primary hover:text-white transition-colors"
              >
                EDIT
              </button>
            )}
          </div>

          {updateSuccess && (
            <div className="mb-6 p-3 bg-green-50 border-2 border-green-500 font-mono text-xs font-bold text-green-700 uppercase">
              Profile updated successfully!
            </div>
          )}

          {updateError && (
            <div className="mb-6 p-3 bg-red-50 border-2 border-red-500 font-mono text-xs font-bold text-red-600 uppercase">
              {(updateError as ApiErrorResponse)?.response?.data?.detail ||
                (updateError as ApiErrorResponse)?.response?.data?.message ||
                "Failed to update profile."}
            </div>
          )}

          <div className="grid grid-cols-1 md:grid-cols-2 gap-8 md:gap-y-12">
            <div className="space-y-2">
              <label className="font-mono text-xs font-bold uppercase text-secondary block">First Name</label>
              {isEditing ? (
                <div>
                  <input
                    type="text"
                    className={`w-full bg-surface border-2 px-4 py-2 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors duration-200 ${errors.firstName ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                    {...register("firstName", {
                      required: "First name is required",
                      pattern: { value: /^[A-Za-z\s]+$/, message: "Must contain only characters and spaces" },
                    })}
                  />
                  {errors.firstName && (
                    <span className="text-red-500 font-mono text-[10px] font-bold uppercase mt-1 block">
                      {errors.firstName.message as string}
                    </span>
                  )}
                </div>
              ) : (
                <div className="font-mono text-lg font-bold border-b-2 border-primary pb-2 uppercase">
                  {currentUser.firstName}
                </div>
              )}
            </div>

            <div className="space-y-2">
              <label className="font-mono text-xs font-bold uppercase text-secondary block">Last Name</label>
              {isEditing ? (
                <div>
                  <input
                    type="text"
                    className={`w-full bg-surface border-2 px-4 py-2 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors duration-200 ${errors.lastName ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                    {...register("lastName", {
                      required: "Last name is required",
                      pattern: { value: /^[A-Za-z\s]+$/, message: "Must contain only characters and spaces" },
                    })}
                  />
                  {errors.lastName && (
                    <span className="text-red-500 font-mono text-[10px] font-bold uppercase mt-1 block">
                      {errors.lastName.message as string}
                    </span>
                  )}
                </div>
              ) : (
                <div className="font-mono text-lg font-bold border-b-2 border-primary pb-2 uppercase">
                  {currentUser.lastName}
                </div>
              )}
            </div>

            <div className="space-y-2">
              <label className="font-mono text-xs font-bold uppercase text-secondary block">Phone Number</label>
              {isEditing ? (
                <div>
                  <input
                    type="tel"
                    maxLength={10}
                    onInput={(e) => {
                      e.currentTarget.value = e.currentTarget.value.replace(/\D/g, "");
                    }}
                    className={`w-full bg-surface border-2 px-4 py-2 font-mono text-sm focus:outline-none focus:ring-0 focus:bg-white transition-colors duration-200 ${errors.phone ? "border-red-500 focus:border-red-500" : "border-primary focus:border-primary"}`}
                    {...register("phone", {
                      required: "Phone number is required",
                      pattern: { value: /^\d{10}$/, message: "Must be exactly 10 digits" },
                    })}
                  />
                  {errors.phone && (
                    <span className="text-red-500 font-mono text-[10px] font-bold uppercase mt-1 block">
                      {errors.phone.message as string}
                    </span>
                  )}
                </div>
              ) : (
                <div className="font-mono text-lg font-bold border-b-2 border-primary pb-2 uppercase">
                  {currentUser.phoneNumber}
                </div>
              )}
            </div>

            <div className="space-y-2">
              <label className="font-mono text-xs font-bold uppercase text-secondary block">
                Email Address <span className="text-[9px] text-zinc-400 font-normal ml-2">(Cannot be changed)</span>
              </label>
              <div className="font-mono text-lg font-bold border-b-2 border-zinc-200 text-zinc-500 pb-2 uppercase break-all">
                {currentUser.email}
              </div>
            </div>
          </div>

          <div className="md:hidden mt-10 space-y-4">
            {isEditing ? (
              <>
                <button
                  type="submit"
                  disabled={isUpdating}
                  className="w-full bg-primary text-white font-mono text-sm uppercase font-bold tracking-widest border-2 border-primary px-4 py-3 hover:bg-white hover:text-black hover:shadow-[4px_4px_0_0_#000] transition-all disabled:opacity-50"
                >
                  {isUpdating ? "SAVING..." : "SAVE CHANGES"}
                </button>
                <button
                  type="button"
                  onClick={() => setIsEditing(false)}
                  className="w-full font-mono text-sm uppercase font-bold tracking-widest border-2 border-transparent px-4 py-3 hover:bg-surface-container-low transition-colors"
                >
                  CANCEL
                </button>
              </>
            ) : (
              <button
                type="button"
                onClick={() => setIsEditing(true)}
                className="w-full font-mono text-sm uppercase font-bold tracking-widest border-2 border-primary px-4 py-3 hover:bg-primary hover:text-white transition-colors"
              >
                EDIT PROFILE
              </button>
            )}
          </div>
        </form>
      </main>
    </div>
  );
}
