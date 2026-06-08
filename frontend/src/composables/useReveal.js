import { onMounted, onBeforeUnmount } from 'vue'

/**
 * useReveal — wires an IntersectionObserver to add `.revealed` to
 * every element with the class `.reveal-on-scroll` inside the given
 * root element (defaults to the whole document body).
 */
export function useReveal() {
  let observer = null

  onMounted(() => {
    const targets = document.querySelectorAll('.reveal-on-scroll')
    if (!targets.length) return

    observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('revealed')
            observer.unobserve(entry.target)
          }
        })
      },
      { threshold: 0.12, rootMargin: '0px 0px -40px 0px' }
    )

    targets.forEach((el) => observer.observe(el))
  })

  onBeforeUnmount(() => {
    if (observer) observer.disconnect()
  })
}
