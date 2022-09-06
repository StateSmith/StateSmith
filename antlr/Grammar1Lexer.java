// Generated from Grammar1.g4 by ANTLR 4.9.2
import org.antlr.v4.runtime.Lexer;
import org.antlr.v4.runtime.CharStream;
import org.antlr.v4.runtime.Token;
import org.antlr.v4.runtime.TokenStream;
import org.antlr.v4.runtime.*;
import org.antlr.v4.runtime.atn.*;
import org.antlr.v4.runtime.dfa.DFA;
import org.antlr.v4.runtime.misc.*;

@SuppressWarnings({"all", "warnings", "unchecked", "unused", "cast"})
public class Grammar1Lexer extends Lexer {
	static { RuntimeMetaData.checkVersion("4.9.2", RuntimeMetaData.VERSION); }

	protected static final DFA[] _decisionToDFA;
	protected static final PredictionContextCache _sharedContextCache =
		new PredictionContextCache();
	public static final int
		T__0=1, T__1=2, T__2=3, T__3=4, T__4=5, T__5=6, T__6=7, T__7=8, T__8=9, 
		T__9=10, T__10=11, T__11=12, LINE_ENDER=13, IDENTIFIER=14, LINE_COMMENT=15, 
		STAR_COMMENT=16, STRING=17, TICK_STRING=18, DIGIT=19, PERIOD=20, COMMA=21, 
		PLUS=22, DASH=23, COLON=24, GT=25, LT=26, OTHER_SYMBOLS=27, HWS=28;
	public static String[] channelNames = {
		"DEFAULT_TOKEN_CHANNEL", "HIDDEN"
	};

	public static String[] modeNames = {
		"DEFAULT_MODE"
	};

	private static String[] makeRuleNames() {
		return new String[] {
			"T__0", "T__1", "T__2", "T__3", "T__4", "T__5", "T__6", "T__7", "T__8", 
			"T__9", "T__10", "T__11", "LINE_ENDER", "IDENTIFIER", "NOT_NL_CR", "LINE_COMMENT", 
			"STAR_COMMENT", "ESCAPED_CHAR", "NON_QUOTE_CHAR", "STRING_CHAR", "STRING", 
			"TICK_STRING", "IDENTIFIER_NON_DIGIT", "DIGIT", "PERIOD", "COMMA", "PLUS", 
			"DASH", "COLON", "GT", "LT", "OTHER_SYMBOLS", "HWS"
		};
	}
	public static final String[] ruleNames = makeRuleNames();

	private static String[] makeLiteralNames() {
		return new String[] {
			null, "'$STATEMACHINE'", "'$NOTES'", "'$ORTHO'", "'#'", "'('", "')'", 
			"'['", "']'", "'/'", "'{'", "'}'", "'e'", null, null, null, null, null, 
			null, null, "'.'", "','", "'+'", "'-'", "':'", "'>'", "'<'"
		};
	}
	private static final String[] _LITERAL_NAMES = makeLiteralNames();
	private static String[] makeSymbolicNames() {
		return new String[] {
			null, null, null, null, null, null, null, null, null, null, null, null, 
			null, "LINE_ENDER", "IDENTIFIER", "LINE_COMMENT", "STAR_COMMENT", "STRING", 
			"TICK_STRING", "DIGIT", "PERIOD", "COMMA", "PLUS", "DASH", "COLON", "GT", 
			"LT", "OTHER_SYMBOLS", "HWS"
		};
	}
	private static final String[] _SYMBOLIC_NAMES = makeSymbolicNames();
	public static final Vocabulary VOCABULARY = new VocabularyImpl(_LITERAL_NAMES, _SYMBOLIC_NAMES);

	/**
	 * @deprecated Use {@link #VOCABULARY} instead.
	 */
	@Deprecated
	public static final String[] tokenNames;
	static {
		tokenNames = new String[_SYMBOLIC_NAMES.length];
		for (int i = 0; i < tokenNames.length; i++) {
			tokenNames[i] = VOCABULARY.getLiteralName(i);
			if (tokenNames[i] == null) {
				tokenNames[i] = VOCABULARY.getSymbolicName(i);
			}

			if (tokenNames[i] == null) {
				tokenNames[i] = "<INVALID>";
			}
		}
	}

	@Override
	@Deprecated
	public String[] getTokenNames() {
		return tokenNames;
	}

	@Override

	public Vocabulary getVocabulary() {
		return VOCABULARY;
	}


	public Grammar1Lexer(CharStream input) {
		super(input);
		_interp = new LexerATNSimulator(this,_ATN,_decisionToDFA,_sharedContextCache);
	}

	@Override
	public String getGrammarFileName() { return "Grammar1.g4"; }

	@Override
	public String[] getRuleNames() { return ruleNames; }

	@Override
	public String getSerializedATN() { return _serializedATN; }

	@Override
	public String[] getChannelNames() { return channelNames; }

	@Override
	public String[] getModeNames() { return modeNames; }

	@Override
	public ATN getATN() { return _ATN; }

	public static final String _serializedATN =
		"\3\u608b\ua72a\u8133\ub9ed\u417c\u3be7\u7786\u5964\2\36\u00cb\b\1\4\2"+
		"\t\2\4\3\t\3\4\4\t\4\4\5\t\5\4\6\t\6\4\7\t\7\4\b\t\b\4\t\t\t\4\n\t\n\4"+
		"\13\t\13\4\f\t\f\4\r\t\r\4\16\t\16\4\17\t\17\4\20\t\20\4\21\t\21\4\22"+
		"\t\22\4\23\t\23\4\24\t\24\4\25\t\25\4\26\t\26\4\27\t\27\4\30\t\30\4\31"+
		"\t\31\4\32\t\32\4\33\t\33\4\34\t\34\4\35\t\35\4\36\t\36\4\37\t\37\4 \t"+
		" \4!\t!\4\"\t\"\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3\2\3"+
		"\2\3\3\3\3\3\3\3\3\3\3\3\3\3\3\3\4\3\4\3\4\3\4\3\4\3\4\3\4\3\5\3\5\3\6"+
		"\3\6\3\7\3\7\3\b\3\b\3\t\3\t\3\n\3\n\3\13\3\13\3\f\3\f\3\r\3\r\3\16\6"+
		"\16u\n\16\r\16\16\16v\3\17\3\17\3\17\7\17|\n\17\f\17\16\17\177\13\17\3"+
		"\20\3\20\3\21\3\21\3\21\3\21\7\21\u0087\n\21\f\21\16\21\u008a\13\21\3"+
		"\22\3\22\3\22\3\22\7\22\u0090\n\22\f\22\16\22\u0093\13\22\3\22\3\22\3"+
		"\22\3\23\3\23\3\23\3\24\3\24\3\25\3\25\5\25\u009f\n\25\3\26\3\26\7\26"+
		"\u00a3\n\26\f\26\16\26\u00a6\13\26\3\26\3\26\3\27\3\27\7\27\u00ac\n\27"+
		"\f\27\16\27\u00af\13\27\3\27\3\27\3\30\3\30\3\31\3\31\3\32\3\32\3\33\3"+
		"\33\3\34\3\34\3\35\3\35\3\36\3\36\3\37\3\37\3 \3 \3!\3!\3\"\6\"\u00c8"+
		"\n\"\r\"\16\"\u00c9\3\u0091\2#\3\3\5\4\7\5\t\6\13\7\r\b\17\t\21\n\23\13"+
		"\25\f\27\r\31\16\33\17\35\20\37\2!\21#\22%\2\'\2)\2+\23-\24/\2\61\25\63"+
		"\26\65\27\67\309\31;\32=\33?\34A\35C\36\3\2\t\4\2\f\f\17\17\3\2$$\3\2"+
		"))\6\2&&C\\aac|\3\2\62;\f\2##\'(,,\61\61<=??AA``~~\u0080\u0080\4\2\13"+
		"\13\"\"\2\u00ce\2\3\3\2\2\2\2\5\3\2\2\2\2\7\3\2\2\2\2\t\3\2\2\2\2\13\3"+
		"\2\2\2\2\r\3\2\2\2\2\17\3\2\2\2\2\21\3\2\2\2\2\23\3\2\2\2\2\25\3\2\2\2"+
		"\2\27\3\2\2\2\2\31\3\2\2\2\2\33\3\2\2\2\2\35\3\2\2\2\2!\3\2\2\2\2#\3\2"+
		"\2\2\2+\3\2\2\2\2-\3\2\2\2\2\61\3\2\2\2\2\63\3\2\2\2\2\65\3\2\2\2\2\67"+
		"\3\2\2\2\29\3\2\2\2\2;\3\2\2\2\2=\3\2\2\2\2?\3\2\2\2\2A\3\2\2\2\2C\3\2"+
		"\2\2\3E\3\2\2\2\5S\3\2\2\2\7Z\3\2\2\2\ta\3\2\2\2\13c\3\2\2\2\re\3\2\2"+
		"\2\17g\3\2\2\2\21i\3\2\2\2\23k\3\2\2\2\25m\3\2\2\2\27o\3\2\2\2\31q\3\2"+
		"\2\2\33t\3\2\2\2\35x\3\2\2\2\37\u0080\3\2\2\2!\u0082\3\2\2\2#\u008b\3"+
		"\2\2\2%\u0097\3\2\2\2\'\u009a\3\2\2\2)\u009e\3\2\2\2+\u00a0\3\2\2\2-\u00a9"+
		"\3\2\2\2/\u00b2\3\2\2\2\61\u00b4\3\2\2\2\63\u00b6\3\2\2\2\65\u00b8\3\2"+
		"\2\2\67\u00ba\3\2\2\29\u00bc\3\2\2\2;\u00be\3\2\2\2=\u00c0\3\2\2\2?\u00c2"+
		"\3\2\2\2A\u00c4\3\2\2\2C\u00c7\3\2\2\2EF\7&\2\2FG\7U\2\2GH\7V\2\2HI\7"+
		"C\2\2IJ\7V\2\2JK\7G\2\2KL\7O\2\2LM\7C\2\2MN\7E\2\2NO\7J\2\2OP\7K\2\2P"+
		"Q\7P\2\2QR\7G\2\2R\4\3\2\2\2ST\7&\2\2TU\7P\2\2UV\7Q\2\2VW\7V\2\2WX\7G"+
		"\2\2XY\7U\2\2Y\6\3\2\2\2Z[\7&\2\2[\\\7Q\2\2\\]\7T\2\2]^\7V\2\2^_\7J\2"+
		"\2_`\7Q\2\2`\b\3\2\2\2ab\7%\2\2b\n\3\2\2\2cd\7*\2\2d\f\3\2\2\2ef\7+\2"+
		"\2f\16\3\2\2\2gh\7]\2\2h\20\3\2\2\2ij\7_\2\2j\22\3\2\2\2kl\7\61\2\2l\24"+
		"\3\2\2\2mn\7}\2\2n\26\3\2\2\2op\7\177\2\2p\30\3\2\2\2qr\7g\2\2r\32\3\2"+
		"\2\2su\t\2\2\2ts\3\2\2\2uv\3\2\2\2vt\3\2\2\2vw\3\2\2\2w\34\3\2\2\2x}\5"+
		"/\30\2y|\5/\30\2z|\5\61\31\2{y\3\2\2\2{z\3\2\2\2|\177\3\2\2\2}{\3\2\2"+
		"\2}~\3\2\2\2~\36\3\2\2\2\177}\3\2\2\2\u0080\u0081\n\2\2\2\u0081 \3\2\2"+
		"\2\u0082\u0083\7\61\2\2\u0083\u0084\7\61\2\2\u0084\u0088\3\2\2\2\u0085"+
		"\u0087\5\37\20\2\u0086\u0085\3\2\2\2\u0087\u008a\3\2\2\2\u0088\u0086\3"+
		"\2\2\2\u0088\u0089\3\2\2\2\u0089\"\3\2\2\2\u008a\u0088\3\2\2\2\u008b\u008c"+
		"\7\61\2\2\u008c\u008d\7,\2\2\u008d\u0091\3\2\2\2\u008e\u0090\13\2\2\2"+
		"\u008f\u008e\3\2\2\2\u0090\u0093\3\2\2\2\u0091\u0092\3\2\2\2\u0091\u008f"+
		"\3\2\2\2\u0092\u0094\3\2\2\2\u0093\u0091\3\2\2\2\u0094\u0095\7,\2\2\u0095"+
		"\u0096\7\61\2\2\u0096$\3\2\2\2\u0097\u0098\7^\2\2\u0098\u0099\13\2\2\2"+
		"\u0099&\3\2\2\2\u009a\u009b\n\3\2\2\u009b(\3\2\2\2\u009c\u009f\5%\23\2"+
		"\u009d\u009f\5\'\24\2\u009e\u009c\3\2\2\2\u009e\u009d\3\2\2\2\u009f*\3"+
		"\2\2\2\u00a0\u00a4\7$\2\2\u00a1\u00a3\5)\25\2\u00a2\u00a1\3\2\2\2\u00a3"+
		"\u00a6\3\2\2\2\u00a4\u00a2\3\2\2\2\u00a4\u00a5\3\2\2\2\u00a5\u00a7\3\2"+
		"\2\2\u00a6\u00a4\3\2\2\2\u00a7\u00a8\7$\2\2\u00a8,\3\2\2\2\u00a9\u00ad"+
		"\t\4\2\2\u00aa\u00ac\5)\25\2\u00ab\u00aa\3\2\2\2\u00ac\u00af\3\2\2\2\u00ad"+
		"\u00ab\3\2\2\2\u00ad\u00ae\3\2\2\2\u00ae\u00b0\3\2\2\2\u00af\u00ad\3\2"+
		"\2\2\u00b0\u00b1\t\4\2\2\u00b1.\3\2\2\2\u00b2\u00b3\t\5\2\2\u00b3\60\3"+
		"\2\2\2\u00b4\u00b5\t\6\2\2\u00b5\62\3\2\2\2\u00b6\u00b7\7\60\2\2\u00b7"+
		"\64\3\2\2\2\u00b8\u00b9\7.\2\2\u00b9\66\3\2\2\2\u00ba\u00bb\7-\2\2\u00bb"+
		"8\3\2\2\2\u00bc\u00bd\7/\2\2\u00bd:\3\2\2\2\u00be\u00bf\7<\2\2\u00bf<"+
		"\3\2\2\2\u00c0\u00c1\7@\2\2\u00c1>\3\2\2\2\u00c2\u00c3\7>\2\2\u00c3@\3"+
		"\2\2\2\u00c4\u00c5\t\7\2\2\u00c5B\3\2\2\2\u00c6\u00c8\t\b\2\2\u00c7\u00c6"+
		"\3\2\2\2\u00c8\u00c9\3\2\2\2\u00c9\u00c7\3\2\2\2\u00c9\u00ca\3\2\2\2\u00ca"+
		"D\3\2\2\2\f\2v{}\u0088\u0091\u009e\u00a4\u00ad\u00c9\2";
	public static final ATN _ATN =
		new ATNDeserializer().deserialize(_serializedATN.toCharArray());
	static {
		_decisionToDFA = new DFA[_ATN.getNumberOfDecisions()];
		for (int i = 0; i < _ATN.getNumberOfDecisions(); i++) {
			_decisionToDFA[i] = new DFA(_ATN.getDecisionState(i), i);
		}
	}
}