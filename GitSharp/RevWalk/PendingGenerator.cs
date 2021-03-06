/*
 * Copyright (C) 2008, Shawn O. Pearce <spearce@spearce.org>
 * Copyright (C) 2009, Henon <meinrad.recheis@gmail.com>
 * Copyright (C) 2009, Gil Ran <gilrun@gmail.com>
 *
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or
 * without modification, are permitted provided that the following
 * conditions are met:
 *
 * - Redistributions of source code must retain the above copyright
 *   notice, this list of conditions and the following disclaimer.
 *
 * - Redistributions in binary form must reproduce the above
 *   copyright notice, this list of conditions and the following
 *   disclaimer in the documentation and/or other materials provided
 *   with the distribution.
 *
 * - Neither the name of the Git Development Community nor the
 *   names of its contributors may be used to endorse or promote
 *   products derived from this software without specific prior
 *   written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
 * CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT,
 * STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using GitSharp.RevWalk.Filter;
using GitSharp.Exceptions;
namespace GitSharp.RevWalk
{


    /**
     * Default (and first pass) RevCommit Generator implementation for RevWalk.
     * <p>
     * This generator starts from a set of one or more commits and process them in
     * descending (newest to oldest) commit time order. Commits automatically cause
     * their parents to be enqueued for further processing, allowing the entire
     * commit graph to be walked. A {@link RevFilter} may be used to select a subset
     * of the commits and return them to the caller.
     */
    public class PendingGenerator : Generator
    {
        public static int PARSED = RevWalk.PARSED;

        public static int SEEN = RevWalk.SEEN;

        public static int UNINTERESTING = RevWalk.UNINTERESTING;

        /**
         * Number of additional commits to scan after we think we are done.
         * <p>
         * This small buffer of commits is scanned to ensure we didn't miss anything
         * as a result of clock skew when the commits were made. We need to set our
         * constant to 1 additional commit due to the use of a pre-increment
         * operator when accessing the value.
         */
        public static int OVER_SCAN = 5 + 1;

        /** A commit near the end of time, to initialize {@link #last} with. */
        private static RevCommit INIT_LAST;

        static PendingGenerator()
        {
            INIT_LAST = new RevCommit(ObjectId.ZeroId);
            INIT_LAST.commitTime = int.MaxValue;
        }

        private RevWalk walker;

        private DateRevQueue pending;

        private RevFilter filter;

        private int output;

        /** Last commit produced to the caller from {@link #next()}. */
        private RevCommit last = INIT_LAST;

        /**
         * Number of commits we have remaining in our over-scan allotment.
         * <p>
         * Only relevant if there are {@link #UNINTERESTING} commits in the
         * {@link #pending} queue.
         */
        public int overScan = OVER_SCAN;

        public bool canDispose;

        public PendingGenerator(RevWalk w, DateRevQueue p,
                 RevFilter f, int @out)
        {
            walker = w;
            pending = p;
            filter = f;
            output = @out;
            canDispose = true;
        }

        public override int outputType()
        {
            return output | SORT_COMMIT_TIME_DESC;
        }

        public override RevCommit next()
        {
            try
            {
                for (; ; )
                {
                    RevCommit c = pending.next();
                    if (c == null)
                    {
                        walker.curs.release();
                        return null;
                    }

                    bool produce;
                    if ((c.flags & UNINTERESTING) != 0)
                        produce = false;
                    else
                        produce = filter.include(walker, c);

                    foreach (RevCommit p in c.parents)
                    {
                        if ((p.flags & SEEN) != 0)
                            continue;
                        if ((p.flags & PARSED) == 0)
                            p.parse(walker);
                        p.flags |= SEEN;
                        pending.add(p);
                    }
                    walker.carryFlagsImpl(c);

                    if ((c.flags & UNINTERESTING) != 0)
                    {
                        if (pending.everbodyHasFlag(UNINTERESTING))
                        {
                            RevCommit n = pending.peek();
                            if (n != null && n.commitTime >= last.commitTime)
                            {
                                // This is too close to call. The next commit we
                                // would pop is dated after the last one produced.
                                // We have to keep going to ensure that we carry
                                // flags as much as necessary.
                                //
                                overScan = OVER_SCAN;
                            }
                            else if (--overScan == 0)
                                throw StopWalkException.INSTANCE;
                        }
                        else
                        {
                            overScan = OVER_SCAN;
                        }
                        if (canDispose)
                            c.dispose();
                        continue;
                    }

                    if (produce)
                        return last = c;
                    else if (canDispose)
                        c.dispose();
                }
            }
            catch (StopWalkException)
            {
                walker.curs.release();
                pending.clear();
                return null;
            }
        }
    }
}
