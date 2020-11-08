import numpy as np
import re
import sklearn.svm

pattern = '^.*\\[(..),(..),(..),(..),(..)\\].*\\[(\\d+)\\],.*$'

f = open('aceOrTJ.txt')
lines = map(lambda line: re.match(pattern, line), f.readlines())
f.close()

hands = [(tuple(line[i] for i in [1, 2, 3, 4, 5]), int(line[6]), line[0]) for line in lines]

# Cards are U, V, T, J, A for cards in order by rank
# Input vector:
# [ 0]: U == 2
# [ 1]: U == 3
# [ 2]: U == 4
# [ 3]: U == 5
# [ 4]: U == 6
# [ 5]: U == 7
# [ 6]: V == 3
# [ 7]: V == 4
# [ 8]: V == 5
# [ 9]: V == 6
# [10]: V == 7
# [11]: V == 8
# [12]: V == 9
# [13]: U,V have the same suit
# [14]: U,TJ have the same suit
# [15]: V,TJ have the same suit
# [16]: U,A have the same suit
# [17]: V,A have the same suit
# Output vector:
# 1 if we should keep TJ, 0 if we only keep Ace only
def make_input_and_output(hand):
    x = np.zeros(18)
    suit = {'..23456789TJQKA'.find(card[0]): card[1] for card in hand[0]}
    U, V, T, J, A = sorted(suit.keys())
    x[U - 2] += 1
    x[V + 3] += 1
    suit_checks = [(U, V), (U, T), (V, T), (U, A), (V, A)]
    for i, (a, b) in enumerate(suit_checks):
        if suit[a] == suit[b]:
            x[13 + i] += 1
    y = bin(hand[1]).count('1') - 1
    return (x, y)

X, Y = zip(*map(make_input_and_output, hands))
X, Y = np.array(X), np.array(Y)

svm = sklearn.svm.LinearSVC(verbose=True)
svm.fit(X, Y)

print()

print(svm.score(X, Y))

def test_vector(v):
    pos = np.array([x for (x, y) in zip(X, Y) if y >= 0.5])
    neg = np.array([x for (x, y) in zip(X, Y) if y < 0.5])
    print('TJ: %f ~ %f' % (min(v.dot(pos.T)[0]), max(v.dot(pos.T)[0])))
    print('A : %f ~ %f' % (min(v.dot(neg.T)[0]), max(v.dot(neg.T)[0])))
