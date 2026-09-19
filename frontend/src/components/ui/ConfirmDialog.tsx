import { useEffect, useId, useRef } from "react";
import { AlertTriangle, X } from "lucide-react";

interface ConfirmDialogProps {
  isOpen: boolean;
  message: string;
  title?: string;
  confirmLabel?: string;
  cancelLabel?: string;
  pendingLabel?: string;
  isConfirming?: boolean;
  onConfirm: () => unknown;
  onCancel: () => unknown;
}

/**
 * Neo-brutalist confirmation dialog used for destructive actions
 * (e.g. deleting a category). Replaces the native `window.confirm` popup.
 */
export default function ConfirmDialog({
  isOpen,
  message,
  title = "Confirm Delete",
  confirmLabel = "Delete",
  cancelLabel = "Cancel",
  pendingLabel = "Working…",
  isConfirming = false,
  onConfirm,
  onCancel,
}: ConfirmDialogProps) {
  const titleId = useId();
  const messageId = useId();
  const cancelButtonRef = useRef<HTMLButtonElement>(null);
  const onCancelRef = useRef(onCancel);

  useEffect(() => {
    onCancelRef.current = onCancel;
  });

  useEffect(() => {
    if (!isOpen) return;

    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape" && !isConfirming) onCancelRef.current();
    };

    const previousOverflow = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    window.addEventListener("keydown", handleKeyDown);
    cancelButtonRef.current?.focus();

    return () => {
      document.body.style.overflow = previousOverflow;
      window.removeEventListener("keydown", handleKeyDown);
    };
  }, [isOpen, isConfirming]);

  if (!isOpen) return null;

  return (
    <div
      className="fixed inset-0 bg-black/80 z-60 flex items-center justify-center p-4 backdrop-blur-sm"
      onClick={() => {
        if (!isConfirming) onCancel();
      }}
    >
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby={titleId}
        aria-describedby={messageId}
        onClick={(event) => event.stopPropagation()}
        className="bg-white border-4 border-red-600 shadow-[12px_12px_0_0_#000] p-8 w-full max-w-md relative animate-in fade-in zoom-in duration-200 max-h-[90vh] overflow-y-auto"
      >
        <button
          type="button"
          onClick={() => onCancel()}
          disabled={isConfirming}
          title="Close"
          className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
        >
          <X className="w-6 h-6" />
        </button>

        <div className="flex items-start gap-4 pr-10">
          <div className="w-12 h-12 border-2 border-red-600 bg-red-100 flex items-center justify-center shrink-0">
            <AlertTriangle className="w-6 h-6 text-red-600" />
          </div>
          <h2
            id={titleId}
            className="font-display text-2xl sm:text-3xl font-black uppercase tracking-tighter"
          >
            {title}
          </h2>
        </div>

        <p id={messageId} className="font-mono text-sm mt-6 text-secondary">
          {message}
        </p>

        <div className="flex flex-col-reverse sm:flex-row gap-3 mt-8">
          <button
            ref={cancelButtonRef}
            type="button"
            onClick={() => onCancel()}
            disabled={isConfirming}
            className="flex-1 bg-white text-primary font-mono font-black uppercase py-4 px-6 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none hover:bg-primary hover:text-white transition-all cursor-pointer disabled:opacity-40 disabled:pointer-events-none"
          >
            {cancelLabel}
          </button>
          <button
            type="button"
            onClick={() => onConfirm()}
            disabled={isConfirming}
            className="flex-1 bg-red-600 text-white font-mono font-black uppercase py-4 px-6 border-2 border-red-600 shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none hover:bg-white hover:text-red-700 transition-all cursor-pointer disabled:opacity-50 disabled:pointer-events-none"
          >
            {isConfirming ? pendingLabel : confirmLabel}
          </button>
        </div>
      </div>
    </div>
  );
}
