// Backend-dən gələn PDF stream-ini brauzerdə blob URL-ə çevirib yeni tab-da açır.
window.openPdfStream = async (streamRef) => {
    const bytes = await streamRef.arrayBuffer();
    const blob = new Blob([bytes], { type: "application/pdf" });
    const url = URL.createObjectURL(blob);
    window.open(url, "_blank");
};
