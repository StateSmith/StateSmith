// import MainClass

open class Spec2SmBase {
    companion object {
        fun trace_guard(message: String, b: Boolean): Boolean {
            return MainClass.traceGuard(message, b)
        }
    }
}
