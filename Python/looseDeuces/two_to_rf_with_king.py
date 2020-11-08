import numpy as np
import re
import sklearn.svm

pattern = '^.*\\[(..),(..),(..),(..),(..)\\].*\\[(\\d+)\\],.*$'

f = open('twoToRFWithKing.txt')
lines = filter(lambda x: x != None, map(lambda line: re.match(pattern, line), f.readlines()))
f.close()

hands = [(tuple(line[i] for i in [1, 2, 3, 4, 5]), int(line[6]), line[0]) for line in lines]

TK = list(filter(lambda h: 'Tc,Kc' in h[2], hands))
JK = list(filter(lambda h: 'Jc,Kc' in h[2], hands))
QK = list(filter(lambda h: 'Qc,Kc' in h[2], hands))

# Non-RF cards are U, V, W, sorted by rank. Z is the non-King in the royal flush draw.
# Input vector:
# [ 0]: U == 3
# [ 1]: U == 4
# [ 2]: U == 5
# [ 3]: U == 6
# [ 4]: U == 7
# [ 5]: U == 8
# [ 6]: V == 4
# [ 7]: V == 5
# [ 8]: V == 6
# [ 9]: V == 7
# [10]: V == 8
# [11]: V == 9
# [12]: W == 5
# [13]: W == 6
# [14]: W == 7
# [15]: W == 8
# [16]: W == 9
# [17]: W == T
# [18]: W == J
# [19]: W == Q
# [20]: W == K
# [21]: W == A
# [22]: U,V have the same suit
# [23]: U,W have the same suit
# [24]: V,W have the same suit
# [25]: U,K have the same suit
# [26]: V,K have the same suit
# [27]: W,K have the same suit
# Output vector:
# 1 if we should keep the TK/JK/QK, 0 if we discard everything
def make_input_and_output(hand):
    x = np.zeros(28)
    suit = {'..23456789TJQKA'.find(card[0]): card[1] for card in hand[0]}
    rf_draw = {k: v for (k, v) in suit.items() if v == suit[13] and 10 <= k <= 13}
    U, V, W = sorted(filter(lambda k: k not in rf_draw, suit.keys()))
    x[U - 3] += 1
    x[V + 2] += 1
    x[W + 7] += 1
    suit_checks = [(U, V), (U, W), (V, W), (U, 13), (V, 13), (W, 13)]
    for i, (a, b) in enumerate(suit_checks):
       if suit[a] == suit[b]:
           x[22 + i] += 1
    y = 1 if hand[1] != 0 else 0
    return (x, y)

XT, YT = [np.array(A) for A in zip(*map(make_input_and_output, TK))]
svm = sklearn.svm.LinearSVC(verbose=True)
svm.fit(XT, YT)
print()
print(svm.score(XT, YT))

XJ, YJ = [np.array(A) for A in zip(*map(make_input_and_output, JK))]
svm = sklearn.svm.LinearSVC(verbose=True)
svm.fit(XJ, YJ)
print()
print(svm.score(XJ, YJ))

XQ, YQ = [np.array(A) for A in zip(*map(make_input_and_output, QK))]
svm = sklearn.svm.LinearSVC(verbose=True)
svm.fit(XQ, YQ)
print()
print(svm.score(XQ, YQ))

def test_vector(v, X, Y):
   pos = np.array([x for (x, y) in zip(X, Y) if y >= 0.5])
   neg = np.array([x for (x, y) in zip(X, Y) if y < 0.5])
   print('TJ: %f ~ %f' % (min(v.dot(pos.T)[0]), max(v.dot(pos.T)[0])))
   print('A : %f ~ %f' % (min(v.dot(neg.T)[0]), max(v.dot(neg.T)[0])))
