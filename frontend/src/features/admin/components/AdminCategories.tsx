import React, { useState, type FormEvent } from "react";
import { Plus, X, Edit, Trash2, Link as LinkIcon, RefreshCw, Image as ImageIcon } from "lucide-react";
import { useCategories } from "../../categories/hooks/useCategories";
import type { Category } from "../../categories/types/Category";
import {
  useAssignSubCategories,
  useCreateCategory,
  useDeleteCategory,
  useUnassignSubCategories,
  useUpdateCategory,
} from "../hooks/useAdminCategoryMutations";
import { resolveImageUrl } from "../../../lib/imageUrl";
import ConfirmDialog from "../../../components/ui/ConfirmDialog";

const shortId = (id: string) => id.slice(0, 8).toUpperCase();

export const AdminCategories: React.FC = () => {
  const { data: categoriesList, isPending, isError, refetch } = useCategories();

  const createCategory = useCreateCategory();
  const updateCategory = useUpdateCategory();
  const deleteCategory = useDeleteCategory();
  const assignSub = useAssignSubCategories();
  const unassignSub = useUnassignSubCategories();

  const [isAddingCategory, setIsAddingCategory] = useState(false);
  const [newCategory, setNewCategory] = useState<{
    name: string;
    image: File | null;
    previewUrl: string;
  }>({ name: "", image: null, previewUrl: "" });

  const [isEditingCategory, setIsEditingCategory] = useState<{
    id: string;
    name: string;
    imageUrl: string | null;
    newImage: File | null;
    previewUrl: string;
  } | null>(null);

  const [isAssigningCategory, setIsAssigningCategory] = useState<{
    id: string;
    name: string;
    initialSubIds: string[];
    selectedSubIds: string[];
  } | null>(null);

  const [actionError, setActionError] = useState<string | null>(null);

  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null);

  const isMutating =
    createCategory.isPending ||
    updateCategory.isPending ||
    deleteCategory.isPending ||
    assignSub.isPending ||
    unassignSub.isPending;

  const handlePickNewCategoryImage = (file: File | undefined) => {
    if (!file) return;
    setNewCategory((prev) => ({ ...prev, image: file, previewUrl: URL.createObjectURL(file) }));
  };

  const handlePickEditCategoryImage = (file: File | undefined) => {
    if (!file) return;
    setIsEditingCategory((prev) => (prev ? { ...prev, newImage: file, previewUrl: URL.createObjectURL(file) } : prev));
  };

  const handleAddCategory = async (e: FormEvent) => {
    e.preventDefault();
    setActionError(null);
    try {
      await createCategory.mutateAsync({
        name: newCategory.name.trim(),
        image: newCategory.image ?? undefined,
      });
      setIsAddingCategory(false);
      setNewCategory({ name: "", image: null, previewUrl: "" });
    } catch {
      setActionError("Failed to create category. It may already exist.");
    }
  };

  const handleEditCategory = async (e: FormEvent) => {
    e.preventDefault();
    if (!isEditingCategory) return;
    setActionError(null);
    try {
      await updateCategory.mutateAsync({
        categoryId: isEditingCategory.id,
        name: isEditingCategory.name.trim(),
        image: isEditingCategory.newImage ?? undefined,
      });
      setIsEditingCategory(null);
    } catch {
      setActionError("Failed to update category.");
    }
  };

  const requestDeleteCategory = (category: Category) => {
    setActionError(null);
    setDeleteTarget({ id: category.id, name: category.categoryName });
  };

  const handleConfirmDeleteCategory = async () => {
    if (!deleteTarget) return;
    setActionError(null);
    try {
      await deleteCategory.mutateAsync({ categoryId: deleteTarget.id });
      setDeleteTarget(null);
    } catch {
      setDeleteTarget(null);
      setActionError("Failed to delete category.");
    }
  };

  const openAssignModal = (category: Category) => {
    const initialSubIds = (category.subcategories ?? []).map((sub) => sub.id);
    setIsAssigningCategory({
      id: category.id,
      name: category.categoryName,
      initialSubIds,
      selectedSubIds: initialSubIds,
    });
  };

  const handleAssignSubcategories = async (e: FormEvent) => {
    e.preventDefault();
    if (!isAssigningCategory) return;
    setActionError(null);
    const { id, initialSubIds, selectedSubIds } = isAssigningCategory;
    const toAssign = selectedSubIds.filter((subId) => !initialSubIds.includes(subId));
    const toUnassign = initialSubIds.filter((subId) => !selectedSubIds.includes(subId));
    try {
      if (toAssign.length > 0) {
        await assignSub.mutateAsync({ categoryId: id, subCategoryIds: toAssign });
      }
      if (toUnassign.length > 0) {
        await unassignSub.mutateAsync({ categoryId: id, subCategoryIds: toUnassign });
      }
      setIsAssigningCategory(null);
    } catch {
      setActionError("Failed to update sub-category assignments.");
    }
  };

  return (
    <div className="flex flex-col gap-6">
      {actionError && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">{actionError}</p>
          <button
            type="button"
            onClick={() => setActionError(null)}
            className="p-1 border-2 border-red-600 text-red-700 hover:bg-red-600 hover:text-white transition-colors cursor-pointer"
            title="Dismiss"
          >
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {isError && (
        <div className="bg-red-50 border-4 border-red-600 p-4 flex items-center justify-between gap-4">
          <p className="font-mono text-xs font-bold uppercase text-red-700">Failed to load categories.</p>
          <button
            type="button"
            onClick={() => void refetch()}
            className="bg-red-600 text-white font-mono text-xs font-bold uppercase py-2 px-4 border-2 border-red-600 flex items-center gap-2 hover:bg-white hover:text-red-700 transition-colors cursor-pointer"
          >
            <RefreshCw className="w-4 h-4" /> Retry
          </button>
        </div>
      )}
      <div className="flex justify-end">
        <button
          type="button"
          onClick={() => {
            setActionError(null);
            setIsAddingCategory(true);
          }}
          className="bg-primary text-white font-mono font-bold uppercase py-3 px-6 border-2 border-primary flex items-center gap-2 shadow-[4px_4px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 hover:bg-white hover:text-primary transition-all active:translate-x-2 active:translate-y-2 active:shadow-none"
        >
          <Plus className="w-5 h-5" /> Add Category
        </button>
      </div>

      {/* Add Category Modal */}
      {isAddingCategory && (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
          <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-8 w-full max-w-md relative animate-in fade-in zoom-in duration-200 max-h-[90vh] overflow-y-auto">
            <button
              onClick={() => setIsAddingCategory(false)}
              className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all"
            >
              <X className="w-6 h-6" />
            </button>
            <h2 className="font-display text-3xl font-black uppercase tracking-tighter mb-6">Add Category</h2>
            <form onSubmit={handleAddCategory} className="flex flex-col gap-6">
              <div className="flex flex-col gap-2">
                <label className="font-mono text-sm font-bold uppercase">Name</label>
                <input
                  required
                  type="text"
                  value={newCategory.name}
                  onChange={(e) => setNewCategory({ ...newCategory, name: e.target.value })}
                  className="w-full border-2 border-primary bg-surface p-4 font-mono focus:outline-none focus:bg-primary focus:text-white transition-colors"
                  placeholder="e.g. Shoes"
                />
              </div>
              <div className="flex flex-col gap-2">
                <label className="font-mono text-sm font-bold uppercase">Image (Optional)</label>
                <div className="relative w-full border-2 border-primary border-dashed bg-surface p-6 flex flex-col items-center justify-center gap-2 hover:bg-surface-container transition-colors cursor-pointer group">
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) => handlePickNewCategoryImage(e.target.files?.[0])}
                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
                  />
                  {newCategory.previewUrl ? (
                    <div className="w-full flex items-center justify-between border-2 border-primary bg-white p-2">
                      <span className="font-mono text-xs truncate max-w-60 text-primary">
                        {newCategory.image?.name ?? "Image selected"}
                      </span>
                      <div className="w-8 h-8 border-2 border-primary bg-surface overflow-hidden shrink-0">
                        <img src={newCategory.previewUrl} alt="Preview" className="w-full h-full object-cover" />
                      </div>
                    </div>
                  ) : (
                    <>
                      <Plus className="w-8 h-8 text-secondary group-hover:text-primary transition-colors" />
                      <span className="font-mono text-xs font-bold text-secondary group-hover:text-primary transition-colors text-center">
                        Click or drag image here
                      </span>
                    </>
                  )}
                </div>
              </div>
              <button
                type="submit"
                disabled={createCategory.isPending}
                className="mt-4 w-full bg-primary text-white font-mono font-black uppercase py-4 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none hover:bg-white hover:text-primary transition-all disabled:opacity-50 disabled:pointer-events-none"
              >
                {createCategory.isPending ? "Saving…" : "Save Category"}
              </button>
            </form>
          </div>
        </div>
      )}
      {/* Edit Category Modal */}
      {isEditingCategory && (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
          <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-8 w-full max-w-md relative animate-in fade-in zoom-in duration-200 max-h-[90vh] overflow-y-auto">
            <button
              onClick={() => setIsEditingCategory(null)}
              className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all"
            >
              <X className="w-6 h-6" />
            </button>
            <h2 className="font-display text-3xl font-black uppercase tracking-tighter mb-6">Edit Category</h2>
            <form onSubmit={handleEditCategory} className="flex flex-col gap-6">
              <div className="flex flex-col gap-2">
                <label className="font-mono text-sm font-bold uppercase">Name</label>
                <input
                  required
                  type="text"
                  value={isEditingCategory.name}
                  onChange={(e) => setIsEditingCategory({ ...isEditingCategory, name: e.target.value })}
                  className="w-full border-2 border-primary bg-surface p-4 font-mono focus:outline-none focus:bg-primary focus:text-white transition-colors"
                />
              </div>
              <div className="flex flex-col gap-2">
                <label className="font-mono text-sm font-bold uppercase">New Image (Optional)</label>
                <div className="relative w-full border-2 border-primary border-dashed bg-surface p-6 flex flex-col items-center justify-center gap-2 hover:bg-surface-container transition-colors cursor-pointer group">
                  <input
                    type="file"
                    accept="image/*"
                    onChange={(e) => handlePickEditCategoryImage(e.target.files?.[0])}
                    className="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10"
                  />
                  {isEditingCategory.previewUrl ? (
                    <div className="w-full flex items-center justify-between border-2 border-primary bg-white p-2">
                      <span className="font-mono text-xs truncate max-w-60 text-primary">
                        {isEditingCategory.newImage?.name ?? "New image"}
                      </span>
                      <div className="w-8 h-8 border-2 border-primary bg-surface overflow-hidden shrink-0">
                        <img src={isEditingCategory.previewUrl} alt="Preview" className="w-full h-full object-cover" />
                      </div>
                    </div>
                  ) : isEditingCategory.imageUrl ? (
                    <div className="w-full flex items-center justify-between border-2 border-primary bg-white p-2">
                      <span className="font-mono text-xs truncate max-w-60 text-primary">Current image</span>
                      <div className="w-8 h-8 border-2 border-primary bg-surface overflow-hidden shrink-0">
                        <img
                          src={resolveImageUrl(isEditingCategory.imageUrl)}
                          alt="Current"
                          className="w-full h-full object-cover"
                        />
                      </div>
                    </div>
                  ) : (
                    <>
                      <Plus className="w-8 h-8 text-secondary group-hover:text-primary transition-colors" />
                      <span className="font-mono text-xs font-bold text-secondary group-hover:text-primary transition-colors text-center">
                        Click or drag new image here
                      </span>
                    </>
                  )}
                </div>
              </div>
              <button
                type="submit"
                disabled={updateCategory.isPending}
                className="mt-4 w-full bg-primary text-white font-mono font-black uppercase py-4 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none hover:bg-white hover:text-primary transition-all disabled:opacity-50 disabled:pointer-events-none"
              >
                {updateCategory.isPending ? "Saving…" : "Update Category"}
              </button>
            </form>
          </div>
        </div>
      )}
      {/* Assign Categories Modal */}
      {isAssigningCategory && (
        <div className="fixed inset-0 bg-black/80 z-50 flex items-center justify-center p-4 backdrop-blur-sm">
          <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] p-8 w-full max-w-md relative animate-in fade-in zoom-in duration-200 max-h-[90vh] overflow-y-auto">
            <button
              onClick={() => setIsAssigningCategory(null)}
              className="absolute top-4 right-4 text-primary hover:bg-surface-container p-2 border-2 border-transparent hover:border-primary transition-all"
            >
              <X className="w-6 h-6" />
            </button>
            <h2 className="font-display text-3xl font-black uppercase tracking-tighter mb-6">Assign Categories</h2>
            <p className="font-mono text-sm mb-4">
              Select sub-categories for <strong>{isAssigningCategory.name}</strong>:
            </p>
            <form onSubmit={handleAssignSubcategories} className="flex flex-col gap-6">
              <div className="flex flex-col gap-2 max-h-60 overflow-y-auto border-2 border-primary p-4 bg-surface">
                {(categoriesList ?? [])
                  .filter((category) => category.id !== isAssigningCategory.id)
                  .map((category) => (
                    <label
                      key={category.id}
                      className="flex items-center gap-3 font-mono cursor-pointer hover:bg-white p-2 border-2 border-transparent hover:border-primary transition-colors"
                    >
                      <input
                        type="checkbox"
                        checked={isAssigningCategory.selectedSubIds.includes(category.id)}
                        onChange={(e) => {
                          setIsAssigningCategory({
                            ...isAssigningCategory,
                            selectedSubIds: e.target.checked
                              ? [...isAssigningCategory.selectedSubIds, category.id]
                              : isAssigningCategory.selectedSubIds.filter((id) => id !== category.id),
                          });
                        }}
                        className="w-5 h-5 accent-primary border-2 border-primary"
                      />
                      <span className="uppercase font-bold">{category.categoryName}</span>
                    </label>
                  ))}
              </div>
              <p className="font-mono text-[10px] text-secondary">
                Newly checked categories will be assigned; unchecked ones will be unassigned.
              </p>
              <button
                type="submit"
                disabled={assignSub.isPending || unassignSub.isPending}
                className="mt-4 w-full bg-primary text-white font-mono font-black uppercase py-4 border-2 border-primary shadow-[6px_6px_0_0_#000] hover:shadow-[2px_2px_0_0_#000] hover:translate-x-1 hover:translate-y-1 active:translate-x-2 active:translate-y-2 active:shadow-none hover:bg-white hover:text-primary transition-all disabled:opacity-50 disabled:pointer-events-none"
              >
                {assignSub.isPending || unassignSub.isPending ? "Saving…" : "Save Assignments"}
              </button>
            </form>
          </div>
        </div>
      )}
      {/* Delete Category Confirm Modal */}
      <ConfirmDialog
        isOpen={deleteTarget !== null}
        title="Delete Category"
        message={
          deleteTarget
            ? `Are you sure you want to delete "${deleteTarget.name}" (ID: ${shortId(deleteTarget.id)})? This action cannot be undone.`
            : ""
        }
        confirmLabel="Yes, Delete"
        pendingLabel="Deleting…"
        isConfirming={deleteCategory.isPending}
        onConfirm={() => handleConfirmDeleteCategory()}
        onCancel={() => setDeleteTarget(null)}
      />
      <div className="bg-white border-4 border-primary shadow-[12px_12px_0_0_#000] overflow-x-auto w-full">
        <table className="w-full text-left font-mono border-collapse min-w-212.5">
          <thead className="bg-primary text-white">
            <tr>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm w-24">Image</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">ID</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Category Name</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm">Sub-Categories</th>
              <th className="p-4 border-b-4 border-primary uppercase tracking-widest text-sm text-right">Actions</th>
            </tr>
          </thead>
          <tbody className="bg-white text-sm font-bold divide-y-2 divide-primary">
            {isPending ? (
              <tr>
                <td colSpan={5} className="p-6 text-center text-secondary font-mono animate-pulse">
                  Loading categories…
                </td>
              </tr>
            ) : (categoriesList ?? []).length === 0 ? (
              <tr>
                <td colSpan={5} className="p-6 text-center text-secondary font-mono">
                  No categories found.
                </td>
              </tr>
            ) : (
              (categoriesList ?? []).map((category) => (
                <tr key={category.id} className="hover:bg-surface-container transition-colors group">
                  <td className="p-2 border-r-2 border-primary">
                    <div className="w-16 h-16 border-2 border-primary bg-surface overflow-hidden">
                      {resolveImageUrl(category.imageUrl) ? (
                        <img
                          src={resolveImageUrl(category.imageUrl)}
                          alt={category.categoryName}
                          className="w-full h-full object-cover"
                        />
                      ) : (
                        <div className="w-full h-full flex items-center justify-center">
                          <ImageIcon className="w-5 h-5 text-secondary" />
                        </div>
                      )}
                    </div>
                  </td>
                  <td className="p-4 border-r-2 border-primary text-secondary">{shortId(category.id)}</td>
                  <td className="p-4 border-r-2 border-primary uppercase">{category.categoryName}</td>
                  <td className="p-4 border-r-2 border-primary">
                    <div className="flex gap-1 flex-wrap">
                      {category.subcategories && category.subcategories.length > 0 ? (
                        category.subcategories.map((sub) => (
                          <span
                            key={sub.id}
                            className="px-2 py-1 text-[10px] bg-surface border-2 border-primary uppercase"
                          >
                            {sub.categoryName}
                          </span>
                        ))
                      ) : (
                        <span className="text-zinc-400 font-normal italic">None</span>
                      )}
                    </div>
                  </td>
                  <td className="p-4 text-right">
                    <div className="flex justify-end gap-2 opacity-100 sm:opacity-0 sm:group-hover:opacity-100 transition-opacity">
                      <button
                        onClick={() => openAssignModal(category)}
                        disabled={isMutating}
                        className="p-2 bg-surface text-primary border-2 border-primary hover:bg-primary hover:text-white transition-colors disabled:opacity-40 disabled:pointer-events-none"
                        title="Assign Subcategories"
                      >
                        <LinkIcon className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => {
                          setActionError(null);
                          setIsEditingCategory({
                            id: category.id,
                            name: category.categoryName,
                            imageUrl: category.imageUrl ?? null,
                            newImage: null,
                            previewUrl: "",
                          });
                        }}
                        disabled={isMutating}
                        className="p-2 bg-surface text-primary border-2 border-primary hover:bg-black hover:text-white transition-colors disabled:opacity-40 disabled:pointer-events-none"
                        title="Edit Category"
                      >
                        <Edit className="w-4 h-4" />
                      </button>
                      <button
                        onClick={() => requestDeleteCategory(category)}
                        disabled={isMutating}
                        className="p-2 bg-red-100 text-red-600 border-2 border-red-600 hover:bg-red-600 hover:text-white transition-colors disabled:opacity-40 disabled:pointer-events-none"
                        title="Delete Category"
                      >
                        <Trash2 className="w-4 h-4" />
                      </button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default AdminCategories;
