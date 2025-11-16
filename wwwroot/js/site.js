// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Site-wide helper functions

function sharePost(postId, threadId) {
  try {
    const url = `${location.origin}/Forum/ThreadDetail/${threadId}#post-${postId}`;

    // If navigator.share available (mobile/native), use it
    if (navigator.share) {
      navigator
        .share({
          title: document.title,
          text: "Bu yorumu inceleyin",
          url: url,
        })
        .catch(() => {
          // ignore share errors
        });
      return;
    }

    // Fallback: copy to clipboard and notify
    if (navigator.clipboard && navigator.clipboard.writeText) {
      navigator.clipboard
        .writeText(url)
        .then(() => {
          showToast("Link panoya kopyalandı");
        })
        .catch(() => {
          window.prompt("Linki kopyalayın:", url);
        });
    } else {
      window.prompt("Linki kopyalayın:", url);
    }
  } catch (e) {
    console.error("Share error", e);
  }
}

function showToast(message, duration = 2200) {
  let toast = document.createElement("div");
  toast.textContent = message;
  toast.className =
    "fixed bottom-6 right-6 bg-gray-900 text-white px-4 py-2 rounded shadow-lg opacity-95";
  document.body.appendChild(toast);
  setTimeout(() => {
    toast.remove();
  }, duration);
}
