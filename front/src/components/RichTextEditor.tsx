import { useState } from 'react';
import { useEditor, EditorContent } from '@tiptap/react';
import StarterKit from '@tiptap/starter-kit';
import Underline from '@tiptap/extension-underline';
import Placeholder from '@tiptap/extension-placeholder';

interface ToolbarBtnProps {
  active?: boolean;
  disabled?: boolean;
  onClick: () => void;
  title: string;
  children: React.ReactNode;
}

function ToolbarBtn({ active, disabled, onClick, title, children }: ToolbarBtnProps) {
  return (
    <button
      type="button"
      title={title}
      onClick={onClick}
      disabled={disabled}
      className={`
        w-7 h-7 flex items-center justify-center rounded text-xs transition-colors select-none
        ${active
          ? 'bg-violet-100 dark:bg-violet-950/60 text-violet-700 dark:text-violet-300'
          : 'text-gray-500 dark:text-gray-400 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-700 dark:hover:text-gray-200'
        }
        disabled:opacity-30 disabled:cursor-not-allowed
      `}
    >
      {children}
    </button>
  );
}

function Sep() {
  return <span className="w-px h-4 bg-gray-200 dark:bg-gray-700 mx-0.5 self-center shrink-0" />;
}

interface Props {
  value: string;
  onChange: (html: string) => void;
  placeholder?: string;
}

export default function RichTextEditor({ value, onChange, placeholder }: Props) {
  const [sourceMode, setSourceMode] = useState(false);
  const [sourceHtml, setSourceHtml] = useState(value);

  const editor = useEditor({
    extensions: [
      StarterKit,
      Underline,
      Placeholder.configure({ placeholder: placeholder ?? 'Descrição (opcional)' }),
    ],
    content: value || '',
    onUpdate: ({ editor }) => {
      const html = editor.getHTML();
      onChange(html === '<p></p>' ? '' : html);
    },
    editorProps: {
      attributes: {
        class: 'min-h-[140px] px-3 py-2.5 focus:outline-none text-sm',
      },
    },
  });

  const handleToggleSource = () => {
    if (!editor) return;
    if (!sourceMode) {
      setSourceHtml(editor.getHTML());
    } else {
      editor.commands.setContent(sourceHtml, { emitUpdate: false });
      const html = sourceHtml;
      onChange(html === '<p></p>' ? '' : html);
    }
    setSourceMode(s => !s);
  };

  if (!editor) return null;

  return (
    <div className="rounded-xl border border-gray-200 dark:border-gray-700 overflow-hidden bg-white dark:bg-gray-900 focus-within:ring-2 focus-within:ring-violet-500 focus-within:border-transparent transition-all">
      {/* Toolbar */}
      <div className="flex flex-wrap items-center gap-0.5 px-2 py-1.5 border-b border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 min-h-[2.5rem]">
        {!sourceMode && (
          <>
            <ToolbarBtn active={editor.isActive('bold')} onClick={() => editor.chain().focus().toggleBold().run()} title="Negrito (Ctrl+B)">
              <span className="font-bold">B</span>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('italic')} onClick={() => editor.chain().focus().toggleItalic().run()} title="Itálico (Ctrl+I)">
              <span className="italic font-serif">I</span>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('underline')} onClick={() => editor.chain().focus().toggleUnderline().run()} title="Sublinhado (Ctrl+U)">
              <span className="underline">U</span>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('strike')} onClick={() => editor.chain().focus().toggleStrike().run()} title="Tachado">
              <span className="line-through">S</span>
            </ToolbarBtn>

            <Sep />

            <ToolbarBtn active={editor.isActive('heading', { level: 1 })} onClick={() => editor.chain().focus().toggleHeading({ level: 1 }).run()} title="Título 1">
              <span className="font-bold text-[10px] tracking-tight">H1</span>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('heading', { level: 2 })} onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()} title="Título 2">
              <span className="font-bold text-[10px] tracking-tight">H2</span>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('heading', { level: 3 })} onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()} title="Título 3">
              <span className="font-bold text-[10px] tracking-tight">H3</span>
            </ToolbarBtn>

            <Sep />

            <ToolbarBtn active={editor.isActive('bulletList')} onClick={() => editor.chain().focus().toggleBulletList().run()} title="Lista com marcadores">
              <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor">
                <circle cx="2" cy="4" r="1.4" />
                <rect x="5" y="3" width="9" height="2" rx="1" />
                <circle cx="2" cy="8" r="1.4" />
                <rect x="5" y="7" width="9" height="2" rx="1" />
                <circle cx="2" cy="12" r="1.4" />
                <rect x="5" y="11" width="9" height="2" rx="1" />
              </svg>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('orderedList')} onClick={() => editor.chain().focus().toggleOrderedList().run()} title="Lista numerada">
              <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor">
                <text x="0" y="5.5" fontSize="5" fontFamily="monospace" fontWeight="bold">1.</text>
                <text x="0" y="9.8" fontSize="5" fontFamily="monospace" fontWeight="bold">2.</text>
                <text x="0" y="14.1" fontSize="5" fontFamily="monospace" fontWeight="bold">3.</text>
                <rect x="6" y="3.5" width="9" height="2" rx="1" />
                <rect x="6" y="7.8" width="9" height="2" rx="1" />
                <rect x="6" y="12.1" width="9" height="2" rx="1" />
              </svg>
            </ToolbarBtn>
            <ToolbarBtn active={editor.isActive('blockquote')} onClick={() => editor.chain().focus().toggleBlockquote().run()} title="Citação">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor">
                <path d="M3 21c3 0 7-1 7-8V5c0-1.25-.756-2.017-2-2H4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2 1 0 1 0 1 1v1c0 1-1 2-2 2s-1 .008-1 1.031V20c0 1 0 1 1 1z" />
                <path d="M15 21c3 0 7-1 7-8V5c0-1.25-.757-2.017-2-2h-4c-1.25 0-2 .75-2 1.972V11c0 1.25.75 2 2 2h.75c0 2.25.25 4-2.75 4v3c0 1 0 1 1 1z" />
              </svg>
            </ToolbarBtn>

            <Sep />

            <ToolbarBtn disabled={!editor.can().undo()} onClick={() => editor.chain().focus().undo().run()} title="Desfazer (Ctrl+Z)">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <path d="M9 14 4 9l5-5" />
                <path d="M4 9h10.5a5.5 5.5 0 0 1 5.5 5.5v0a5.5 5.5 0 0 1-5.5 5.5H11" />
              </svg>
            </ToolbarBtn>
            <ToolbarBtn disabled={!editor.can().redo()} onClick={() => editor.chain().focus().redo().run()} title="Refazer (Ctrl+Y)">
              <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round" strokeLinejoin="round">
                <path d="m15 14 5-5-5-5" />
                <path d="M20 9H9.5A5.5 5.5 0 0 0 4 14.5v0A5.5 5.5 0 0 0 9.5 20H13" />
              </svg>
            </ToolbarBtn>
          </>
        )}

        <button
          type="button"
          onClick={handleToggleSource}
          title={sourceMode ? 'Modo visual' : 'Editar HTML'}
          className={`
            ml-auto px-2 py-0.5 rounded text-[10px] font-mono font-semibold transition-colors shrink-0
            ${sourceMode
              ? 'bg-violet-100 dark:bg-violet-950/60 text-violet-700 dark:text-violet-300'
              : 'text-gray-400 dark:text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-700 hover:text-gray-600 dark:hover:text-gray-300'
            }
          `}
        >
          {sourceMode ? 'Visual' : '</>'}
        </button>
      </div>

      {sourceMode ? (
        <textarea
          value={sourceHtml}
          onChange={e => {
            setSourceHtml(e.target.value);
            onChange(e.target.value);
          }}
          className="w-full min-h-[140px] px-3 py-2.5 font-mono text-xs text-gray-900 dark:text-gray-100 bg-white dark:bg-gray-900 focus:outline-none resize-y"
          spellCheck={false}
        />
      ) : (
        <div className="rte-wrap">
          <EditorContent editor={editor} />
        </div>
      )}
    </div>
  );
}
