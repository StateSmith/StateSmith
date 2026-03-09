@MainActor
public class Spec2SmBase {
    static func trace_guard(_ message: String , _ b: Bool) -> Bool {
        MainClass.traceGuard(message, b);
    }
}
